namespace InElonWeTrust.Core.Commands.Definitions
{
    public enum GroupType
    {
        [GroupTypeDescription(":rocket:", "Launches", "information about all SpaceX launches")]
        Launches,

        [GroupTypeDescription(":frame_photo:", "Media", "commands related with Twitter, Flickr and Reddit")]
        Media,

        [GroupTypeDescription(":warning:", "Notifications", "subscribe to get all newest content")]
        Notifications,

        [GroupTypeDescription(":clock4:", "Time zone", "set local timezone to see more precise times in launch information")]
        TimeZone,

        [GroupTypeDescription(":question:", "Miscellaneous", "other commands")]
        Miscellaneous
    }
}
