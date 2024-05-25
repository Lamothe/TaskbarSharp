using System;

namespace TaskbarSharp.Configurator
{

    public delegate double EasingDelegate(double currentTime, double minValue, double maxValue, double duration);

    public class Easings
    {

        // All Animations are here some of them can be found here https://easings.net/en
        public static double Linear(double currentTime, double minHeight, double maxHeight, double duration)
        {
            return maxHeight * currentTime / duration + minHeight;
        }

        public static double ExpoEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime != duration)
            {
                return maxHeight * (-Math.Pow(2.0d, -10.0d * currentTime / duration) + 1.0d) + minHeight;
            }
            return minHeight + maxHeight;
        }

        public static double ExpoEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime != 0.0d)
            {
                return maxHeight * Math.Pow(2.0d, 10.0d * (currentTime / duration - 1.0d)) + minHeight;
            }
            return minHeight;
        }

        public static double ExpoEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime == 0.0d)
            {
                return minHeight;
            }
            if (currentTime == duration)
            {
                return minHeight + maxHeight;
            }
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * Math.Pow(2.0d, 10.0d * (currentTime - 1.0d)) + minHeight;
            }
            double num2 = maxHeight / 2.0d;
            double x = 2.0d;
            double num3 = -10.0d;
            double num4 = currentTime - 1.0d;
            return num2 * (-Math.Pow(x, num3 * num4) + 2.0d) + minHeight;
        }

        public static double ExpoEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return ExpoEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return ExpoEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double CircEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = 1.0d;
            double num2 = currentTime / duration - 1.0d;
            currentTime = num2;
            return maxHeight * Math.Sqrt(num - num2 * currentTime) + minHeight;
        }

        public static double CircEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = 1.0d;
            double num2 = currentTime / duration;
            currentTime = num2;
            double sqrt = Math.Sqrt(num - num2 * currentTime);
            if (double.IsNaN(sqrt))
            {
                sqrt = 0.0d;
            }
            return -maxHeight * (sqrt - 1.0d) + minHeight;
        }

        public static double CircEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return -maxHeight / 2.0d * (Math.Sqrt(1.0d - currentTime * currentTime) - 1.0d) + minHeight;
            }
            double num2 = maxHeight / 2.0d;
            double num3 = 1.0d;
            double num4 = currentTime - 2.0d;
            currentTime = num4;
            return num2 * (Math.Sqrt(num3 - num4 * currentTime) + 1.0d) + minHeight;
        }

        public static double CircEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return CircEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return CircEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double QuadEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = -maxHeight;
            double num2 = currentTime / duration;
            currentTime = num2;
            return num * num2 * (currentTime - 2.0d) + minHeight;
        }

        public static double QuadEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            return maxHeight * num * currentTime + minHeight;
        }

        public static double QuadEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * currentTime * currentTime + minHeight;
            }
            double num2 = -maxHeight / 2.0d;
            double num3 = currentTime - 1.0d;
            currentTime = num3;
            return num2 * (num3 * (currentTime - 2.0d) - 1.0d) + minHeight;
        }

        public static double QuadEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return QuadEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return QuadEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double SineEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            return maxHeight * Math.Sin(currentTime / duration * 1.5707963267948966d) + minHeight;
        }

        public static double SineEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            return -maxHeight * Math.Cos(currentTime / duration * 1.5707963267948966d) + maxHeight + minHeight;
        }

        public static double SineEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * Math.Sin(3.1415926535897931d * currentTime / 2.0d) + minHeight;
            }
            double num2 = -maxHeight / 2.0d;
            double num3 = 3.1415926535897931d;
            double num4 = currentTime - 1.0d;
            return num2 * (Math.Cos(num3 * num4 / 2.0d) - 2.0d) + minHeight;
        }

        public static double SineEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return SineEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return SineEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double CubicEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration - 1.0d;
            currentTime = num;
            return maxHeight * (num * currentTime * currentTime + 1.0d) + minHeight;
        }

        public static double CubicEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            return maxHeight * num * currentTime * currentTime + minHeight;
        }

        public static double CubicEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * currentTime * currentTime * currentTime + minHeight;
            }
            double num2 = maxHeight / 2.0d;
            double num3 = currentTime - 2.0d;
            currentTime = num3;
            return num2 * (num3 * currentTime * currentTime + 2.0d) + minHeight;
        }

        public static double CubicEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return CubicEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return CubicEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double QuartEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = -maxHeight;
            double num2 = currentTime / duration - 1.0d;
            currentTime = num2;
            return num * (num2 * currentTime * currentTime * currentTime - 1.0d) + minHeight;
        }

        public static double QuartEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            return maxHeight * num * currentTime * currentTime * currentTime + minHeight;
        }

        public static double QuartEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * currentTime * currentTime * currentTime * currentTime + minHeight;
            }
            double num2 = -maxHeight / 2.0d;
            double num3 = currentTime - 2.0d;
            currentTime = num3;
            return num2 * (num3 * currentTime * currentTime * currentTime - 2.0d) + minHeight;
        }

        public static double QuartEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return QuartEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return QuartEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double QuintEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration - 1.0d;
            currentTime = num;
            return maxHeight * (num * currentTime * currentTime * currentTime * currentTime + 1.0d) + minHeight;
        }

        public static double QuintEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            return maxHeight * num * currentTime * currentTime * currentTime * currentTime + minHeight;
        }

        public static double QuintEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                return maxHeight / 2.0d * currentTime * currentTime * currentTime * currentTime * currentTime + minHeight;
            }
            double num2 = maxHeight / 2.0d;
            double num3 = currentTime - 2.0d;
            currentTime = num3;
            return num2 * (num3 * currentTime * currentTime * currentTime * currentTime + 2.0d) + minHeight;
        }

        public static double QuintEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return QuintEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return QuintEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double ElasticEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            if (num == 1.0d)
            {
                return minHeight + maxHeight;
            }
            double p = duration * 0.3d;
            double s = p / 4.0d;
            return maxHeight * Math.Pow(2.0d, -10.0d * currentTime) * Math.Sin((currentTime * duration - s) * 6.2831853071795862d / p) + maxHeight + minHeight;
        }

        public static double ElasticEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            if (num == 1.0d)
            {
                return minHeight + maxHeight;
            }
            double p = duration * 0.3d;
            double s = p / 4.0d;
            double x = 2.0d;
            double num2 = 10.0d;
            double num3 = currentTime - 1.0d;
            currentTime = num3;
            return -(maxHeight * Math.Pow(x, num2 * num3) * Math.Sin((currentTime * duration - s) * 6.2831853071795862d / p)) + minHeight;
        }

        public static double ElasticEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num == 2.0d)
            {
                return minHeight + maxHeight;
            }
            double p = duration * 0.44999999999999996d;
            double s = p / 4.0d;
            if (currentTime < 1.0d)
            {
                double num2 = -0.5d;
                double x = 2.0d;
                double num3 = 10.0d;
                double num4 = currentTime - 1.0d;
                currentTime = num4;
                return num2 * (maxHeight * Math.Pow(x, num3 * num4) * Math.Sin((currentTime * duration - s) * 6.2831853071795862d / p)) + minHeight;
            }
            double x2 = 2.0d;
            double num5 = -10.0d;
            double num6 = currentTime - 1.0d;
            currentTime = num6;
            return maxHeight * Math.Pow(x2, num5 * num6) * Math.Sin((currentTime * duration - s) * 6.2831853071795862d / p) * 0.5d + maxHeight + minHeight;
        }

        public static double ElasticEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return ElasticEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return ElasticEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double BounceEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            if (num < 0.36363636363636365d)
            {
                return maxHeight * (7.5625d * currentTime * currentTime) + minHeight;
            }
            if (currentTime < 0.72727272727272729d)
            {
                double num2 = 7.5625d;
                double num3 = currentTime - 0.54545454545454541d;
                currentTime = num3;
                return maxHeight * (num2 * num3 * currentTime + 0.75d) + minHeight;
            }
            if (currentTime < 0.90909090909090906d)
            {
                double num4 = 7.5625d;
                double num5 = currentTime - 0.81818181818181823d;
                currentTime = num5;
                return maxHeight * (num4 * num5 * currentTime + 0.9375d) + minHeight;
            }
            double num6 = 7.5625d;
            double num7 = currentTime - 0.95454545454545459d;
            currentTime = num7;
            return maxHeight * (num6 * num7 * currentTime + 0.984375d) + minHeight;
        }

        public static double BounceEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            return maxHeight - BounceEaseOut(duration - currentTime, 0.0d, maxHeight, duration) + minHeight;
        }

        public static double BounceEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return BounceEaseIn(currentTime * 2.0d, 0.0d, maxHeight, duration) * 0.5d + minHeight;
            }
            return BounceEaseOut(currentTime * 2.0d - duration, 0.0d, maxHeight, duration) * 0.5d + maxHeight * 0.5d + minHeight;
        }

        public static double BounceEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return BounceEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return BounceEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

        public static double BackEaseOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration - 1.0d;
            currentTime = num;
            return maxHeight * (num * currentTime * (2.70158d * currentTime + 1.70158d) + 1.0d) + minHeight;
        }

        public static double BackEaseIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double num = currentTime / duration;
            currentTime = num;
            return maxHeight * num * currentTime * (2.70158d * currentTime - 1.70158d) + minHeight;
        }

        public static double BackEaseInOut(double currentTime, double minHeight, double maxHeight, double duration)
        {
            double s = 1.70158d;
            double num = currentTime / (duration / 2.0d);
            currentTime = num;
            if (num < 1.0d)
            {
                double num2 = maxHeight / 2.0d;
                double num3 = currentTime * currentTime;
                double num4 = s * 1.525d;
                s = num4;
                return num2 * (num3 * ((num4 + 1.0d) * currentTime - s)) + minHeight;
            }
            double num5 = maxHeight / 2.0d;
            double num6 = currentTime - 2.0d;
            currentTime = num6;
            double num7 = num6 * currentTime;
            double num8 = s * 1.525d;
            s = num8;
            return num5 * (num7 * ((num8 + 1.0d) * currentTime + s) + 2.0d) + minHeight;
        }

        public static double BackEaseOutIn(double currentTime, double minHeight, double maxHeight, double duration)
        {
            if (currentTime < duration / 2.0d)
            {
                return BackEaseOut(currentTime * 2.0d, minHeight, maxHeight / 2.0d, duration);
            }
            return BackEaseIn(currentTime * 2.0d - duration, minHeight + maxHeight / 2.0d, maxHeight / 2.0d, duration);
        }

    }
}