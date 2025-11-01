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
        private static bool resumedMusic = false;
        private static bool resumeMusicNextFrame = false;

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

            // var timer = new Timer(10);

            // timer.Elapsed += (source, args) => {
                Audio.CurrentMusicEventInstance.setPaused(true);
            // };

            // timer.Start();

            // Audio.CurrentMusicEventInstance.setVolume(0);
            // Audio.CurrentMusicEventInstance.getTimelinePosition(out var timelinePosition);
            // Audio.CurrentMusicEventInstance.setTimelinePosition((int)(timelinePosition + offset));

            resumedMusic = false;
            resumeMusicNextFrame = false;
        }

        public static void modLevelUnpause(On.Celeste.Level.orig_Unpause orig, Level self) {
            orig(self);
        }

        public static void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            // if(self.unpauseTimer == 0.15f) {
            //     Audio.CurrentMusicEventInstance.setTimelinePosition(pauseTimelinePosition - (int)(self.unpauseTimer * 1000));
            // }
            
            orig(self);

            if(self.unpauseTimer >= 0) {
                Console.WriteLine(self.unpauseTimer);
            }

            if(resumeMusicNextFrame && !resumedMusic) {
                Audio.CurrentMusicEventInstance.setPaused(false);

                Console.WriteLine("resetting");

                // Audio.CurrentMusicEventInstance.setVolume(1);
                resumedMusic = true;
            }

            if(self.unpauseTimer < 0 && !resumedMusic && !self.Paused) 
                resumeMusicNextFrame = true;
        }

        public static int GetTimelinePosition() {
            Audio.CurrentMusicEventInstance.getTimelinePosition(out var timelinePosition);
            
            return timelinePosition;
        }

        public static void MeasurePauseTime(bool paused) {
            var stopwatch = new Stopwatch();
            var counter = 0;
            var lastTimelinePosition = 0;

            Audio.CurrentMusicEventInstance.setPaused(paused);

            var timer = new Timer(0.1);
            timer.AutoReset = true;
            timer.Elapsed += (source, args) => {
                Audio.CurrentMusicEventInstance.getTimelinePosition(out var timelinePosition);

                Console.WriteLine($"{lastTimelinePosition} -> {timelinePosition}");

                // if(paused ? lastTimelinePosition == timelinePosition : timelinePosition != lastTimelinePosition) {
                //     timer.Stop();
                //     stopwatch.Stop();

                //     Console.WriteLine($"it took {stopwatch.Elapsed} to {(paused ? "pause" : "unpause")}");
                // }

                if(counter > 100) {
                    timer.Stop();
                    stopwatch.Stop();

                    Console.WriteLine("bail");
                }

                counter++;
                lastTimelinePosition = timelinePosition;
            };

            timer.Start();
            stopwatch.Start();
        }

        public static IEnumerator PlaySyncedMusic(string path) {            
            Audio.SetMusic(path);

            while(true) {
                Audio.CurrentMusicEventInstance.getPlaybackState(out var playbackState);
                if(playbackState != FMOD.Studio.PLAYBACK_STATE.STARTING) break;

                Logger.Debug("GooberHelper", "waiting for playback state...");

                yield return null;
            }

            while(true) {
                Audio.CurrentMusicEventInstance.getTimelinePosition(out var timelinePosition);
                if(timelinePosition > 0) break;

                Logger.Debug("GooberHelper", "waiting for timeline position...");

                yield return null;
            }

            Logger.Debug("GooberHelper", "done waiting for audio!");

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

/*
var stopwatch = new Stopwatch();
var counter = 0;

Audio.CurrentMusicEventInstance.setPaused(true);

var timer = new System.Timers.Timer(100);
timer.AutoReset = true;
timer.Elapsed += (source, args) => {
    Audio.CurrentMusicEventInstance.getPaused(out var paused);

    if(paused == true) {
        stopwatch.Stop();
        timer.Stop();

        Console.WriteLine($"it took {stopwatch.Elapsed}");
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