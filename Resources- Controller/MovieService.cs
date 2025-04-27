using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using MovieBookRecommendation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieBookRecommendation
{
    public class MovieService
    {
        private List<MovieData> _moviesList;
        private List<MovieFeature> _movieFeatures;
        private ITransformer _transformer;
        private readonly MLContext _mlContext;

        public MovieService()
        {
            _mlContext = new MLContext();
            LoadMovies();
            PreComputeFeatures();
        }

        private void LoadMovies()
        {
            try
            {
                IDataView dataView = _mlContext.Data.LoadFromTextFile<MovieData>(
                    "movie.csv", separatorChar: ',', hasHeader: true);
                _moviesList = _mlContext.Data
                    .CreateEnumerable<MovieData>(dataView, reuseRowObject: false)
                    .Take(20)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar o CSV de filmes: " + ex.Message);
                _moviesList = new List<MovieData>();
            }
        }

        private void PreComputeFeatures()
        {
            // Definir opções para fixar o número de n-grams e evitar schema mismatch
            var textOptions = new TextFeaturizingEstimator.Options
            {
                WordFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 1,    // 1-gramas de palavras
                    UseAllLengths = false // Vetor fixo
                },
                CharFeatureExtractor = new WordBagEstimator.Options
                {
                    NgramLength = 3,    // 3-gramas de caracteres
                    UseAllLengths = false // Vetor fixo
                }
            };

            // Pipeline com opções fixas para garantir consistência
            var pipeline = _mlContext.Transforms.Text
                .FeaturizeText("TitleFeatures", textOptions, nameof(MovieData.Title))
                .Append(_mlContext.Transforms.Text.FeaturizeText("GenresFeatures", textOptions, nameof(MovieData.Genres)))
                .Append(_mlContext.Transforms.Concatenate(
                    outputColumnName: "Features",
                    "TitleFeatures", "GenresFeatures"));

            IDataView trainingData = _mlContext.Data.LoadFromEnumerable(_moviesList);
            _transformer = pipeline.Fit(trainingData);
            var transformedData = _transformer.Transform(trainingData);
            _movieFeatures = _mlContext.Data
                .CreateEnumerable<MovieFeature>(transformedData, reuseRowObject: false)
                .ToList();
        }

        public List<MovieData> RecommendMoviesByContent(string queryTitle, string queryGenre, int topN = 10)
        {
            var query = new MovieData { Title = queryTitle, Genres = queryGenre };
            var queryDv = _mlContext.Data.LoadFromEnumerable(new[] { query });
            var transformedQuery = _transformer.Transform(queryDv);
            var feature = _mlContext.Data
                .CreateEnumerable<MovieFeature>(transformedQuery, reuseRowObject: false)
                .FirstOrDefault();
            if (feature?.Features == null)
                return FallbackRecommendation(queryTitle, queryGenre, topN);

            var results = _movieFeatures.Select(mf => new
            {
                Movie = mf,
                Similarity = CosineSimilarity(feature.Features, mf.Features)
            }).ToList();

            if (results.All(r => r.Similarity == 0))
                return FallbackRecommendation(queryTitle, queryGenre, topN);

            return results
                .OrderByDescending(r => r.Similarity)
                .Take(topN)
                .Select(r => new MovieData
                {
                    MovieId = r.Movie.MovieId,
                    Title = r.Movie.Title,
                    Genres = r.Movie.Genres
                })
                .ToList();
        }

        private List<MovieData> FallbackRecommendation(string queryTitle, string queryGenre, int topN)
        {
            return _moviesList
                .Where(m => m.Title.IndexOf(queryTitle, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            m.Genres.IndexOf(queryGenre, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(topN)
                .ToList();
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                throw new Exception("Vetores devem ter o mesmo tamanho");

            float dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            if (na == 0 || nb == 0)
                return 0;
            return dot / ((float)Math.Sqrt(na) * (float)Math.Sqrt(nb));
        }
    }
}
