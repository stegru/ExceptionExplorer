namespace ExceptionExplorer.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TimeSince
    {
        private static string PartOfDay(DateTime when)
        {
            if ((when.Hour < 4) || (when.Hour >= 22))
            {
                return "night";
            }
            else if (when.Hour < 12)
            {
                return "morning";
            }
            else if (when.Hour < 17)
            {
                return "afternoon";
            }
            else
            {
                return "evening";
            }
        }

        public static string GetTimeAgo(DateTime when)
        {
            string text = "";
            bool ago = true;
            DateTime now = DateTime.Now;
            TimeSpan span = now - when;

            Func<string, string, string> join = (word1, word2) =>
            {
                return string.Format("{0}{1}{2}", word1, word1.Length > 0 ? " " : "", word2);
            };
            Func<string, double, string, string> join2 = (word1, d, word2) =>
            {
                return string.Format("{0}{1}{2} {3}", word1, word1.Length > 0 ? " " : "", d, word2);
            };

            if (when >= now.AddDays(-2))
            {
                // some time yesterday/today
                double mins = span.TotalMinutes;
                string s = "";

                if (span.TotalSeconds < 5)
                {
                    text = "just now";
                    ago = false;
                }
                else if (span.TotalSeconds < 15)
                {
                    text = "a few seconds";
                }
                //else if (mins < 4)
                //{
                //    text = "a few minutes";
                //}
                else if (mins < 20)
                {
                    double near5 = Math.Round(mins / 5.0) * 5;
                    if (near5 > 0 && mins.Roughly(near5, 2.5, out s))
                    {
                        text = join2(s, near5, "minutes");
                    }
                    else
                    {
                        text = mins.RoughlyWhole("a", "minute");
                    }
                }
                else if (mins.Roughly(30, 15, out s))
                {
                    text = join(s, "half an hour");
                }
                //else if (mins.Roughly(60, 15, out s))
                //{
                //    text = string.Format("{0}{1}{2}", s, s.Length > 0 ? " " : "", "an hour");
                //}
                else if (span.TotalHours < 6)
                {
                    text = span.TotalHours.RoughlyWhole("an", "hour");
                }
                else // between 6 hours ago and yesterday
                {
                    string dayPart = PartOfDay(when);
                    ago = false;

                    if ((when.Date == now.Date) || ((now.Hour < 5) && (span.TotalHours < 18)))
                    {
                        // still today
                        if (dayPart == PartOfDay(now))
                        {
                            if (dayPart == "night")
                            {
                                if (when.Hour < 5)
                                {
                                    text = "late last night";
                                }
                                else
                                {
                                    text = "earlier tonight";
                                }
                            }
                            else
                            {
                                text = string.Format("earlier this {0}", dayPart);
                            }
                        }
                        else
                        {
                            text = string.Format("this {0}", dayPart);
                        }
                    }
                    else // yesterday
                    {
                        if (dayPart == "night")
                        {
                            if (when.Hour < 5)
                            {
                                // early in the morning yesterday is not last night
                                text = "over a day";
                                ago = true;
                            }
                            else
                            {
                                text = "last night";
                            }
                        }
                        else
                        {
                            text = "yesterday";
                        }
                    }
                }
            }
            else if (span.TotalDays < 7)
            {
                ago = false;
                text = string.Format("on {0}", when.DayOfWeek);
            }
            else if (span.TotalDays < 20)
            {
                text = (span.TotalDays / 7).RoughlyWhole("a", "week");
            }
            else if (span.TotalDays < 40)
            {
                span.TotalDays.Roughly(31, 12, out text);
                text = join(text, "a month");
            }
            else if ((when.Year == now.Year) && (when.Month == now.Month - 1))
            {
                text = "last month";
                ago = false;
            }
            else
            {
                if (span.TotalDays < 300)
                {
                    text = string.Format("in {0}", when.ToString("MMMM"));
                    ago = false;
                }
                else
                {
                    text = (span.TotalDays / 356).RoughlyWhole("a", "year");
                }

            }


            return ago ? string.Format("{0} ago", text) : text;
        }
    }
}
