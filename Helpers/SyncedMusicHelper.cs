using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class SyncedMusicHelper {
        private static int pauseTimelinePosition;
        private static bool resetMusicVolume = false;

        // public class SyncedMusicInstance {
        //     public string Path;
        //     public bool PauseOnPause;
            
        //     public SyncedMusicInstance(string path) {
        //         Path = path;
        //     }
        // }

        public static void Load() {
            On.Celeste.Level.Pause += modLevelPause;
            On.Celeste.Level.Unpause += modLevelUnpause;
            On.Celeste.Level.Update += modLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.Level.Pause -= modLevelPause;
            On.Celeste.Level.Unpause -= modLevelUnpause;
            On.Celeste.Level.Update -= modLevelUpdate;

            

            // 184.56521739130434
        }

        public static void modLevelPause(On.Celeste.Level.orig_Pause orig, Level self, int startIndex, bool minimal, bool quickReset) {
            orig(self, startIndex, minimal, quickReset);

            Audio.CurrentMusicEventInstance.setVolume(0);
            Audio.CurrentMusicEventInstance.getTimelinePosition(out pauseTimelinePosition);
            resetMusicVolume = false;
        }

        public static void modLevelUnpause(On.Celeste.Level.orig_Unpause orig, Level self) {
            orig(self);

            
        }

        public static void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            if(self.unpauseTimer == 0.15f) {
                Audio.CurrentMusicEventInstance.setTimelinePosition(pauseTimelinePosition - (int)(self.unpauseTimer * 1000));
            }
            
            orig(self);

            if(self.unpauseTimer <= 0 && !resetMusicVolume && !self.Paused) {
                Console.WriteLine("resetting");

                Audio.CurrentMusicEventInstance.setVolume(1);
                resetMusicVolume = true;
            }
        }

        public static IEnumerator PlaySyncedMusic(string path) {            
            Audio.SetMusic(path);

            while(true) {
                Audio.CurrentMusicEventInstance.getPlaybackState(out var buh);
                
                Console.WriteLine("loop");

                if(buh != FMOD.Studio.PLAYBACK_STATE.STARTING) break;

                yield return 0;
            }

            Console.WriteLine("STARTED!");

            yield break;
        }
    }
}

/*
- be able to pause and unpause music when pausing and unpausing the game without delay
    - use setvolume
*/

/*
var stopwatch = new Stopwatch();
var targetPosition = Random.Shared.Range(0, 100000);
var counter = 0;

Audio.CurrentMusicEventInstance.setTimelinePosition(targetPosition);

var timer = new System.Timers.Timer(100);
timer.AutoReset = true;
timer.Elapsed += (source, args) => {
    Audio.CurrentMusicEventInstance.getTimelinePosition(out var timelinePosition);

    Console.WriteLine($"target: {targetPosition}, actual: {timelinePosition}");

    if(timelinePosition == targetPosition) {
        stopwatch.Stop();
        timer.Stop();

        timespans.Add(stopwatch.Elapsed);

        Console.WriteLine($"it took {stopwatch.Elapsed}ms");
    }

    if(counter > 10) {
        timer.Stop();
        stopwatch.Stop();

        Console.WriteLine("bail");
    }

    counter++;
};

timer.Start();
stopwatch.Start();
*/