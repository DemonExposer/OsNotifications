// Code taken from https://github.com/norio-nomura/usernotification

#import <Foundation/Foundation.h>
#import <objc/runtime.h>

#pragma mark - Swizzle NSBundle

NSString *fakeBundleIdentifier = nil;

@implementation NSBundle(swizle)

- (NSString *)__bundleIdentifier {
    if (self == [NSBundle mainBundle]) {
        return fakeBundleIdentifier ? fakeBundleIdentifier : @"com.apple.finder";
    } else {
        return [self __bundleIdentifier];
    }
}

@end

BOOL installNSBundleHook() {
    Class class = objc_getClass("NSBundle");
    if (class) {
        method_exchangeImplementations(class_getInstanceMethod(class, @selector(bundleIdentifier)),
                                       class_getInstanceMethod(class, @selector(__bundleIdentifier)));
        return YES;
    }
    return NO;
}


#pragma mark - NotificationCenterDelegate

@interface NotificationCenterDelegate : NSObject<NSUserNotificationCenterDelegate>

@property (nonatomic, assign) BOOL keepRunning;

@end

@implementation NotificationCenterDelegate

- (void)userNotificationCenter:(NSUserNotificationCenter *)center didDeliverNotification:(NSUserNotification *)notification {
    self.keepRunning = NO;
}

@end


#pragma mark -

BOOL isGuiApplication = NO;

#ifdef __cplusplus
extern "C" {
#endif

void setGuiApplication(BOOL isGuiValue) {
    isGuiApplication = isGuiValue;
}

void showNotification(char *identifier, char *title, char *subtitle, char *informativeText) {
    @autoreleasepool {
        if (installNSBundleHook()) {
            NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
            
            fakeBundleIdentifier = [NSString stringWithUTF8String:identifier];
            
            NSUserNotificationCenter *nc = [NSUserNotificationCenter defaultUserNotificationCenter];
            NotificationCenterDelegate *ncDelegate = [[NotificationCenterDelegate alloc]init];
            ncDelegate.keepRunning = YES;
            nc.delegate = ncDelegate;
            
            NSUserNotification *note = [[NSUserNotification alloc] init];
            note.title = [NSString stringWithUTF8String:title];
            note.subtitle = [NSString stringWithUTF8String:subtitle];
            note.informativeText = [NSString stringWithUTF8String:informativeText];
            
            [nc deliverNotification:note];
            
            while (!isGuiApplication && ncDelegate.keepRunning) {
                [[NSRunLoop currentRunLoop] runUntilDate:[NSDate dateWithTimeIntervalSinceNow:0.1]];
            }
        }
    }
}
    
#ifdef __cplusplus
}
#endif
    
