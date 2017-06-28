using System;
using System.Collections.Generic;

namespace Glicko2
{    
    public static class GlickoCalculator
    {
        private static double VolatilityChange = 0.5;
        private static double ConvergenceTolerance = 0.000001;
        private static double glickoConversion = 173.7178;

        public static GlickoPlayer CalculateRanking(GlickoPlayer competitor, List<GlickoOpponent> opponents, Double? useVolatility = null)
        {
            var variance = ComputeVariance(competitor, opponents);

            var updatedVolatility = useVolatility ?? CalculateNewVolatility(competitor, opponents, variance);
            
            var preratingDeviation = CalculatePreRatingDeviation(competitor.GlickoRatingDeviation, updatedVolatility);

            var newRatingDeviation = CalculateNewRatingDeviation(preratingDeviation, variance);
            var newRating = CalculateNewRating(competitor, opponents, newRatingDeviation);

            competitor.RatingDeviation = ConvertRatingDeviationToOriginal(newRatingDeviation);

            if (opponents.Count == 0) return competitor;

            competitor.Rating = ConvertRatingToOriginal(newRating);
            competitor.Volatility = updatedVolatility;

            return competitor;
        }

        private static double ConvertRatingDeviationToOriginal(double glickoRatingDeviation)
        {
            return glickoConversion * glickoRatingDeviation;
        }

        private static double ConvertRatingToOriginal(double glickoRating)
        {
            return (glickoConversion * glickoRating) + 1500;
        }

        private static double CalculateNewRatingDeviation(double preratingDeviation, double variance)
        {
            return 1 / Math.Sqrt((1 / Double(preratingDeviation)) + (1 / variance));
        }

        private static double CalculateNewRating(GlickoPlayer competitor, List<GlickoOpponent> opponents, double newRatingDeviation)
        {
            var sum = 0.0;

            foreach(var opponent in opponents)
            {
                sum += opponent.GPhi * (opponent.Result - Edeltaphi(competitor.GlickoRating, opponent));
            }

            return competitor.GlickoRating + ((Double(newRatingDeviation)) * sum);
        }

        private static double CalculatePreRatingDeviation(double ratingDeviation, double updatedVolatility)
        {
            return Math.Sqrt(Double(ratingDeviation) + Double(updatedVolatility));
        }

        private static double CalculateNewVolatility(GlickoPlayer competitor, List<GlickoOpponent> opponents, double variance)
        {
            var rankingChange = RatingImprovement(competitor, opponents, variance);
            var rankDeviation = competitor.GlickoRatingDeviation;            
            
            var A = VolatilityTransform(competitor.Volatility);
            var a = VolatilityTransform(competitor.Volatility);

            var k = 1;
            double B = 0.0;

            if (Double(rankingChange) > (Double(competitor.GlickoRatingDeviation) + variance))
            {
                B = Math.Log(Double(rankingChange) - Double(competitor.GlickoRatingDeviation) - variance);
            }

            if (Double(rankingChange) <= (Double(competitor.GlickoRatingDeviation) + variance))
            {                
                var x = VolatilityTransform(competitor.Volatility) - (k * VolatilityChange); 

                while(VolatilityFunction(x, rankingChange, rankDeviation, variance, competitor.Volatility) < 0)
                {
                    k++;
                }                
            }

            B = VolatilityTransform(competitor.Volatility) - (k * VolatilityChange);

            var fA = VolatilityFunction(A, rankingChange, rankDeviation, variance, competitor.Volatility);
            var fB = VolatilityFunction(B, rankingChange, rankDeviation, variance, competitor.Volatility);

            while (Math.Abs(B - A) > ConvergenceTolerance)
            {
                var C = A + ((A - B) * fA / (fB - fA));
                var fC = VolatilityFunction(C, rankingChange, rankDeviation, variance, competitor.Volatility);

                if ((fC * fB) < 0)
                {
                    A = B;
                    fA = fB;
                }
                else
                {
                    fA = fA / 2;
                }

                B = C;
                fB = fC;
            }

            return Math.Exp(A / 2);
        }

        private static double VolatilityTransform(double volatility)
        {
            return Math.Log(Double(volatility));
        }

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static double Double(double x) {
			return x * x;
		}

        private static double VolatilityFunction(double x, double rankingChange, double rankDeviation, double variance, double volatility)
        {
            var leftNumerater = Math.Exp(x) * (Double(rankingChange) - Double(rankDeviation) - variance - Math.Exp(x));
            var leftDenominator = 2 * Double(Double(rankDeviation) + variance + Math.Exp(x));

            var rightNumerater = x - VolatilityTransform(volatility);
            var rightDenomintor = Double(VolatilityChange);

            return (leftNumerater / leftDenominator - rightNumerater / rightDenomintor);
        }

        private static double RatingImprovement(GlickoPlayer competitor, List<GlickoOpponent> opponents, double variance)
        {
            double sum = 0;

            foreach (var opponent in opponents)
            {
                sum += opponent.GPhi * (opponent.Result - Edeltaphi(competitor.GlickoRating, opponent));
            }

            return variance * sum;
        }

        private static double ComputeVariance(GlickoPlayer competitor, List<GlickoOpponent> opponents)
        {
            double sum = 0;
            foreach (var opponent in opponents)
            {
                var eDeltaPhi = Edeltaphi(competitor.GlickoRating, opponent);

                sum += Double(opponent.GPhi) * eDeltaPhi * (1 - eDeltaPhi);
            }

            return Math.Pow(sum, -1);
        }

        private static double Edeltaphi(double playerRating, GlickoPlayer opponent)
        {
            return 1 / (1 + (Math.Exp(-opponent.GPhi * (playerRating - opponent.GlickoRating))));
        }
    }
}