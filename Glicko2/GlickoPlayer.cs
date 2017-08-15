using System;

namespace Glicko2
{
    public class GlickoPlayer
    {
        private static readonly double glickoConversion = 173.7178;

		public static double InitialRating = 1500;
		public static double InitialRD = 350;
		public static double InitialVolatility = 0.06;

		public OtherData OtherData;

		public GlickoPlayer(double? argRating = null, double? argRatingDeviation = null, double? argVolatility = null)
        {
			double rating = argRating ?? InitialRating;
			double ratingDeviation = argRatingDeviation ?? InitialRD;
			double volatility = argVolatility ?? InitialVolatility;

			Rating = rating;
            RatingDeviation = ratingDeviation;
            Volatility = volatility;
        }
        // public string Name { get; set; }
        public double Rating { get; set; }
        public double RatingDeviation { get; set; }
        public double Volatility { get; set; }
        public double GlickoRating { get { return (Rating - InitialRating) / glickoConversion; } }
        public double GlickoRatingDeviation { get { return RatingDeviation / glickoConversion; } }
        public double GPhi { get { return 1 / Math.Sqrt(1 + (3 * GlickoCalculator.Double(GlickoRatingDeviation) / GlickoCalculator.Double(Math.PI))); } }
    }
}