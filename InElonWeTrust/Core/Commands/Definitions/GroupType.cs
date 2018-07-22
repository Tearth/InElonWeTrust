namespace InElonWeTrust.Core.Commands.Definitions
{
    public enum GroupType
    {
        [GroupTypeDescription(":rocket:", "Launches", "information about all SpaceX launches")]
        Launches,

        [GroupTypeDescription(":frame_photo:", "Media", "commands related with Twitter, Flickr and Reddit")]
        Media,

        [GroupTypeDescription(":question:", "Miscellaneous", "other strange commands")]
        Miscellaneous,

        [GroupTypeDescription(":warning:", "Launch notifications", "subscribe to get all newest content")]
        Notifications
    }
}
