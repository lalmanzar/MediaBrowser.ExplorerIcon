using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.ScheduledTasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.ExplorerIcon.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.ExplorerIcon.ScheduledTasks
{
    public class GeneateIcons : IScheduledTask
    {
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;
        private readonly IImageProcessor _imageProcessor;

        public GeneateIcons(IUserManager userManager, ILogger logger, IImageProcessor imageProcessor)
        {
            _userManager = userManager;
            _logger = logger;
            _imageProcessor = imageProcessor;
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Configuration.IsAdministrator);
            if (user == null)
            {
                return;
            }
            var series =
                user.RootFolder.GetRecursiveChildren(user)
                    .OfType<Series>()
                    .Where(x => x.HasImage(ImageType.Primary, 0) && !string.IsNullOrEmpty(x.Path))
                    .ToList();
            var seasons =
                user.RootFolder.GetRecursiveChildren(user)
                    .OfType<Season>()
                    .Where(x => x.HasImage(ImageType.Primary, 0) && !string.IsNullOrEmpty(x.Path))
                    .ToList();
            var movies =
                user.RootFolder.GetRecursiveChildren(user)
                    .OfType<Movie>()
                    .Where(x => x.HasImage(ImageType.Primary, 0) && !string.IsNullOrEmpty(x.Path))
                    .ToList();
            var totalSeries = series.Count();
            var totalSeasons = seasons.Count();
            var totalMovies = movies.Count();
            _logger.Debug("Total Items missing: " + (totalSeries + totalSeasons + totalMovies));

            var currentProgress = 0.0;
            var increment = 100.0/(totalSeries + totalMovies);
            await ProcessItems(progress, series, currentProgress, increment, x => x.Path);
            currentProgress += (totalSeries*increment);
            await ProcessItems(progress, seasons, currentProgress, increment, x => x.Path);
            currentProgress += (totalSeasons*increment);
            await ProcessItems(progress, movies, currentProgress, increment, x => Path.GetDirectoryName(x.Path));
        }

        private async Task ProcessItems<T>(IProgress<double> progress, IEnumerable<T> series, double currentProgress, double increment,
            Func<T, string> pathSelector) where T : BaseItem
        {
            foreach (var item in series)
            {
                var folder = pathSelector(item);
                const ImageType type = ImageType.Primary;
                var imageInfo = item.GetImageInfo(type, 0);
                var destinationFile = Path.Combine(folder, "folder.ico");
                if (Directory.EnumerateFiles(folder).Any(y => Path.GetFileName(y) == "folder.ico") &&
                    File.GetLastWriteTime(destinationFile) > imageInfo.DateModified)
                {
                    continue;
                }
                using (var stream = new MemoryStream())
                {
                    var enhancers = _imageProcessor.ImageEnhancers.Where(i =>
                    {
                        try
                        {
                            return i.Supports(item, type);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Error in image enhancer: {0}", ex,
                                i.GetType().Name);
                            return false;
                        }
                    }).ToList();
                    var options = new ImageProcessingOptions
                    {
                        CropWhiteSpace = false,
                        Enhancers = enhancers,
                        ImageIndex = 0,
                        Image = imageInfo,
                        Item = item,
                        MaxHeight = 280,
                        Quality = 90,
                        OutputFormat = ImageFormat.Png
                    };
                    await _imageProcessor.ProcessImage(options, stream);
                    _logger.Debug("Working with: " + folder);
                    if (File.Exists(destinationFile))
                    {
                        if ((File.GetAttributes(destinationFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            File.SetAttributes(destinationFile, File.GetAttributes(destinationFile) & ~FileAttributes.ReadOnly);
                        }
                        if ((File.GetAttributes(destinationFile) & FileAttributes.Hidden) == FileAttributes.Hidden)
                        {
                            File.SetAttributes(destinationFile, File.GetAttributes(destinationFile) & ~FileAttributes.Hidden);
                        }
                    }
                    IconCreator.CreateIcon(stream, destinationFile);
                    File.SetAttributes(destinationFile, File.GetAttributes(destinationFile) | FileAttributes.Hidden);
                    FolderIcon.CreateIconedFolder(folder, "folder.ico");
                }
                currentProgress += increment;
                progress.Report(currentProgress);
            }
        }


        public IEnumerable<ITaskTrigger> GetDefaultTriggers()
        {
            return new List<ITaskTrigger>
            {
                new IntervalTrigger {Interval = TimeSpan.FromHours(1)}
            };
        }

        public string Name
        {
            get { return "Generate Icons"; }
        }

        public string Description
        {
            get { return "Generate Explorer Icons from Primary Images"; }
        }

        public string Category
        {
            get { return "Library"; }
        }
    }
}