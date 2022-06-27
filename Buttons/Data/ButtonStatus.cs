namespace Buttons.Data
{
    public enum ButtonStatus
    {
        /// <summary>
        /// Uploaded image, but not yet cropped or confirmed.
        /// </summary>
        Uploaded,

        /// <summary>
        /// Cropped and confirmed image and crop values.
        /// </summary>
        Confirmed,

        /// <summary>
        /// Button got printed. (Can only print confirmed buttons.)
        /// </summary>
        Printed,
    }
}
