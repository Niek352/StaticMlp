using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationServerRuntime : IResource, IDisposable
    {
        public const int DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME = 1;

        private readonly Func<IWorldGenerationService> _generationServiceFactory;
        private IWorldGenerationService _generationService;

        public OpenWorldGenerationServerRuntime(
            IWorldGenerationService generationService,
            WorldGenerationRequest defaultRequest,
            int maxChunkGenerationsPerFrame = DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME)
            : this(() => generationService, defaultRequest, maxChunkGenerationsPerFrame)
        {
            _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        }

        public OpenWorldGenerationServerRuntime(
            Func<IWorldGenerationService> generationServiceFactory,
            WorldGenerationRequest defaultRequest,
            int maxChunkGenerationsPerFrame = DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME)
        {
            _generationServiceFactory = generationServiceFactory ?? throw new ArgumentNullException(nameof(generationServiceFactory));
            DefaultRequest = defaultRequest;
            if (maxChunkGenerationsPerFrame <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkGenerationsPerFrame), maxChunkGenerationsPerFrame, "Chunk generation budget must be positive.");

            MaxChunkGenerationsPerFrame = maxChunkGenerationsPerFrame;
        }

        public readonly WorldGenerationRequest DefaultRequest;
        public readonly int MaxChunkGenerationsPerFrame;

        public IWorldGenerationService GenerationService => _generationService ??= _generationServiceFactory();

        public static OpenWorldGenerationServerRuntime CreateDefault(Func<IWorldGenerationService> generationServiceFactory)
        {
            return new OpenWorldGenerationServerRuntime(
                generationServiceFactory,
                new WorldGenerationRequest(
                    new WorldGenerationSeed(12345),
                    WorldChunkBounds.Default,
                    128f,
                    64,
                    0,
                    true,
                    6f),
                DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME);
        }

        public void Dispose()
        {
            if (_generationService is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
