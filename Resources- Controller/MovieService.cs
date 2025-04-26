using Microsoft.ML;
using Microsoft.ML.Data;
using MovieBookRecommendation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MovieBookRecommendation
{
    public class MovieService
    {
        private List<MovieData> _moviesList;
        private List<MovieFeature> _movieFeatures; // Vetores pré-computados
        private ITransformer _transformer;
        private MLContext _mlContext;

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
                // Carrega os filmes do CSV; para teste, pega apenas os primeiros 20 registros
                IDataView dataView = _mlContext.Data.LoadFromTextFile<MovieData>(
                    "movie.csv", separatorChar: ',', hasHeader: true);
                _moviesList = _mlContext.Data.CreateEnumerable<MovieData>(dataView, reuseRowObject: false)
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
            // Cria o pipeline para featurizar os textos (título e gêneros)
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("TitleFeatures", nameof(MovieData.Title))
                .Append(_mlContext.Transforms.Text.FeaturizeText("GenresFeatures", nameof(MovieData.Genres)))
                .Append(_mlContext.Transforms.Concatenate("Features", "TitleFeatures", "GenresFeatures"));

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(_moviesList);
            _transformer = pipeline.Fit(dataView);
            var transformedData = _transformer.Transform(dataView);
            _movieFeatures = _mlContext.Data.CreateEnumerable<MovieFeature>(transformedData, reuseRowObject: false)
                                            .ToList();
        }

        /// <summary>
        /// Recomenda filmes baseados em conteúdo usando ML.NET.
        /// Se os vetores de features não gerarem similaridade positiva (todas zero),
        /// usa um fallback simples de busca por substring.
        /// </summary>
        /// <param name="queryTitle">Termo de busca para o título (ex.: "star")</param>
        /// <param name="queryGenre">Termo de busca para o gênero (ex.: "action")</param>
        /// <param name="topN">Número de recomendações desejadas</param>
        /// <returns>Lista de MovieData com as recomendações</returns>
        public List<MovieData> RecommendMoviesByContent(string queryTitle, string queryGenre, int topN = 10)
        {
            // Cria um objeto query com os termos desejados
            var queryMovie = new MovieData { Title = queryTitle, Genres = queryGenre };
            var queryDataView = _mlContext.Data.LoadFromEnumerable(new List<MovieData> { queryMovie });
            var transformedQueryData = _transformer.Transform(queryDataView);
            var queryFeature = _mlContext.Data.CreateEnumerable<MovieFeature>(transformedQueryData, reuseRowObject: false)
                                              .FirstOrDefault();

            // Se não conseguir gerar features para a query, retorna fallback simples
            if (queryFeature == null || queryFeature.Features == null)
                return FallbackRecommendation(queryTitle, queryGenre, topN);

            // Calcula a similaridade cosseno entre o vetor da query e os vetores pré-computados
            var similarityResults = _movieFeatures.Select(mf => new
            {
                Movie = mf,
                Similarity = CosineSimilarity(queryFeature.Features, mf.Features)
            }).ToList();

            // Se nenhum filme teve similaridade positiva, utiliza fallback simples
            if (similarityResults.All(x => x.Similarity == 0))
                return FallbackRecommendation(queryTitle, queryGenre, topN);

            var results = similarityResults
                .OrderByDescending(x => x.Similarity)
                .Take(topN)
                .Select(x => new MovieData
                {
                    MovieId = x.Movie.MovieId,
                    Title = x.Movie.Title,
                    Genres = x.Movie.Genres
                })
                .ToList();

            return results;
        }

        /// <summary>
        /// Fallback simples: busca os filmes cujo título e gênero contenham os termos informados.
        /// </summary>
        private List<MovieData> FallbackRecommendation(string queryTitle, string queryGenre, int topN)
        {
            return _moviesList
                .Where(m => m.Title.IndexOf(queryTitle, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            m.Genres.IndexOf(queryGenre, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(topN)
                .ToList();
        }

        // Calcula a similaridade cosseno entre dois vetores
        private float CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new Exception("Vetores devem ter o mesmo tamanho");

            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dot += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }
            if (normA == 0 || normB == 0)
                return 0;
            return dot / ((float)Math.Sqrt(normA) * (float)Math.Sqrt(normB));
        }
    }
}
