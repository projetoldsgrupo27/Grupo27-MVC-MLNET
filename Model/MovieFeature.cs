using Microsoft.ML.Data;

namespace MovieBookRecommendation.Models
{
    // Herda de MovieData para manter os dados originais e adiciona o vetor de features
    public class MovieFeature : MovieData
    {
        [VectorType]
        public float[] Features { get; set; }
    }
}