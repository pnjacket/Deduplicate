// See https://aka.ms/new-console-template for more information
using SoundFingerprinting.Builder;
using SoundFingerprinting.InMemory;

Console.WriteLine("Deduplicate MP3s");
var currentPath = AppContext.BaseDirectory;
var di = new DirectoryInfo(currentPath);
var mp3s = di.GetFiles("*.mp3");
var modelService = new InMemoryModelService();
var aSvc = new SoundFingerprinting.Audio.NAudio.NAudioService();
var filesToBeDeleted = new List<FileInfo>();

foreach (var mp3 in mp3s)
{
    var input = aSvc.ReadMonoSamplesFromFile(mp3.FullName, 5512);
    var result = await QueryCommandBuilder.Instance.BuildQueryCommand().From(input).UsingServices(modelService).Query();
    var found = false;

    if (result.ResultEntries.Count() > 0)
    {

        foreach(var r in result.ResultEntries)
        {
            if(r.Audio.Confidence > 0.95)
            {
                found = true;
                Console.WriteLine("--------------");
                Console.WriteLine(r.Audio.Track.Id);
                Console.WriteLine(mp3.Name);
                Console.WriteLine("--------------");

                filesToBeDeleted.Add(mp3);
            }
        }
    }

    if (found)
        continue;

    var hash = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand().From(input).Hash();
    modelService.Insert(new SoundFingerprinting.Data.TrackInfo(mp3.Name, Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), hash);
}

if(filesToBeDeleted.Count() > 0)
{
    Console.Write("Duplicated mp3s were found. Do you want to delete them? [y/N]: ");
    var d = Console.ReadKey();
    Console.WriteLine();

    if(d.Key == ConsoleKey.Y)
    {
        filesToBeDeleted.ForEach(file => file.Delete());
    }
}
else
{
    Console.WriteLine("No duplicated mp3s were found");
}


    
