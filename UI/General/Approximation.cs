namespace ExceptionExplorer.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Globalization;

    public static class Approximation
    {
        /// <summary>Deterimines if a specific number is between two numbers.</summary>
        /// <param name="d">The number in question.</param>
        /// <param name="d1">The low end of the range.</param>
        /// <param name="d2">The hight end of the range.</param>
        /// <returns>True if the number is within the range.</returns>
        public static bool Between(this double d, double d1, double d2)
        {
            return ((d >= d1) && (d <= d2));
        }

        /// <summary>
        /// Determines if a number is close to another number.
        /// </summary>
        /// <param name="d">The number in question.</param>
        /// <param name="target">The target number.</param>
        /// <param name="giveOrTake">The range above/below the target number.</param>
        /// <returns>True if the number is within the range.</returns>
        public static bool GiveOrTake(this double d, double target, double giveOrTake)
        {
            return d.Between(target - giveOrTake, target + giveOrTake);
        }

        /// <summary>Determines if a number is close to another number, and returns some text relating to the distance.</summary>
        /// <param name="d">The number in question.</param>
        /// <param name="number">The target number.</param>
        /// <param name="giveOrTake">The range above/below the target number..</param>
        /// <param name="text">The text.</param>
        /// <returns>True if the number is within the range.</returns>
        public static bool Roughly(this double d, double number, double giveOrTake, out string text)
        {
            text = Roughly(d, number, giveOrTake);
            if (text == null)
            {
                text = "";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if a number is close to another number, and returns some text relating to the distance.
        /// </summary>
        /// <param name="d">The number in question.</param>
        /// <param name="singleArticle">The single article word (eg, "a" or "an").</param>
        /// <param name="subject">The subject.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The text.
        /// </returns>
        public static string RoughlyWhole(this double d, string singleArticle, string subject, out string text)
        {
            return text = d.RoughlyWhole(singleArticle, subject);
        }

        /// <summary>
        /// Determines if a number is close to another number, and returns some text relating to the distance.
        /// </summary>
        /// <param name="d">The number in question.</param>
        /// <param name="singleArticle">The single article word (eg, "a" or "an").</param>
        /// <param name="subject">The subject.</param>
        /// <returns>The text.</returns>
        public static string RoughlyWhole(this double d, string singleArticle, string subject)
        {
            string text = "";
            double whole = Math.Round(d);
            if (whole == 0)
            {
                whole = 1;
            }

            Roughly(d, whole, 1, out text);

            string n = (whole == 1) ? singleArticle : whole.ToString(CultureInfo.CurrentUICulture);
            string plural = (whole == 1) ? "" : "s";
            // examples: "about an hour", "almost a day", "just over 2 months"
            return string.Format("{0}{1}{2} {3}{4}", text, text.Length > 0 ? " " : "", n, subject, plural);
        }


        /// <summary>
        /// Determines if a number is close to another number, and returns some text relating to the distance.
        /// </summary>
        /// <param name="d">The number in question.</param>
        /// <param name="number">The number.</param>
        /// <param name="giveOrTake">The give or take.</param>
        /// <returns>The text.</returns>
        public static string Roughly(this double d, double number, double giveOrTake)
        {
            // -1 - less than
            // -0.5 - almost
            // -0.3 - about
            // -0.1 - 
            //  0   - exactly
            //  0.1 - 
            //  0.3 - about
            //  0.5 - just over
            //  1   - over

            double dist = (d - number) / giveOrTake;

            if (dist < -1 || dist > 1)
            {
                return null;
            }
            else
            {
                string text =
                      dist < -0.5 ? "less than"
                    : dist < -0.3 ? "almost"
                    : dist < -0.1 ? "about"
                    : dist < 0.1 ? ""

                    : dist > 0.5 ? "over"
                    : dist > 0.3 ? "just over"
                    : dist > 0.1 ? "about"
                    : null;

                return text;
            }
        }
    }
}
