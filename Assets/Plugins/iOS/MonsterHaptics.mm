#import <Foundation/Foundation.h>
#import <CoreHaptics/CoreHaptics.h>

static CHHapticEngine *liloEngine;
static id<CHHapticPatternPlayer> liloPlayer;

static BOOL LiloPrepareHaptics(void)
{
    if (@available(iOS 13.0, *))
    {
        if (![[CHHapticEngine capabilitiesForHardware] supportsHaptics]) return NO;
        NSError *error = nil;
        if (liloEngine == nil)
            liloEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
        if (error != nil || liloEngine == nil) return NO;
        [liloEngine startAndReturnError:&error];
        return error == nil;
    }
    return NO;
}

extern "C" void LiloMonsterHapticsStop(void)
{
    if (@available(iOS 13.0, *))
    {
        [liloPlayer stopAtTime:0 error:nil];
        liloPlayer = nil;
    }
}

static void LiloPlayPattern(NSArray<CHHapticEvent *> *events)
{
    if (@available(iOS 13.0, *))
    {
        if (!LiloPrepareHaptics()) return;
        NSError *error = nil;
        CHHapticPattern *pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
        if (error != nil || pattern == nil) return;
        liloPlayer = [liloEngine createPlayerWithPattern:pattern error:&error];
        if (error == nil) [liloPlayer startAtTime:0 error:nil];
    }
}

extern "C" void LiloMonsterHapticsAlert(void)
{
    if (@available(iOS 13.0, *))
    {
        NSArray<CHHapticEventParameter *> *parameters = @[
            [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:0.28f],
            [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.3f]
        ];
        NSMutableArray<CHHapticEvent *> *events = [NSMutableArray arrayWithCapacity:3];
        for (int index = 0; index < 3; index++)
            [events addObject:[[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient
                                                       parameters:parameters relativeTime:index * 0.2]];
        LiloPlayPattern(events);
    }
}

extern "C" void LiloMonsterHapticsChase(void)
{
    if (@available(iOS 13.0, *))
    {
        NSArray<CHHapticEventParameter *> *parameters = @[
            [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:0.72f],
            [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.45f]
        ];
        CHHapticEvent *event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous
                                                       parameters:parameters relativeTime:0 duration:3.0];
        LiloPlayPattern(@[event]);
    }
}
