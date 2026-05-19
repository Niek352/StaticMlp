using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EnemySpawnAudioSystem : ISystem
    {
        private const int SAMPLE_RATE = 22050;
        private AudioClip _sourceActivationClip;
        private AudioClip _enemyAppearanceClip;
        private AudioClip _peakEntryClip;

        public void Init()
        {
            _sourceActivationClip = CreateToneClip("CombatDirectorSourceActivation", 164f, 0.16f, 0.28f);
            _enemyAppearanceClip = CreateToneClip("CombatDirectorEnemyAppearance", 247f, 0.12f, 0.22f);
            _peakEntryClip = CreateToneClip("CombatDirectorPeakEntry", 110f, 0.35f, 0.32f);
        }

        public void Update()
        {
            foreach (var enemy in CW.Query<All<EnemySpawnSource>, AllAdded<EnemySpawnSource>>().Entities())
            {
                var position = ToVector3(enemy.Read<EnemySpawnSource>().SpawnPosition);
                AudioSource.PlayClipAtPoint(_sourceActivationClip, position);
                AudioSource.PlayClipAtPoint(_enemyAppearanceClip, position);
            }

            foreach (var director in CW.Query<All<DirectorState>>().Entities())
                PlayPhaseTransitionAudio(director);
        }

        public void Destroy()
        {
            Object.Destroy(_sourceActivationClip);
            Object.Destroy(_enemyAppearanceClip);
            Object.Destroy(_peakEntryClip);
        }

        private void PlayPhaseTransitionAudio(CW.Entity director)
        {
            ref readonly var state = ref director.Read<DirectorState>();
            if (!director.Has<DirectorAudioPresentationState>())
            {
                director.Set(new DirectorAudioPresentationState
                {
                    LastPhase = state.Phase,
                    IsInitialized = true
                });
                return;
            }

            ref var audio = ref director.Mut<DirectorAudioPresentationState>();
            if (audio.LastPhase == state.Phase)
                return;

            audio.LastPhase = state.Phase;
            audio.IsInitialized = true;
            if (state.Phase == DirectorPhase.Peak)
                AudioSource.PlayClipAtPoint(_peakEntryClip, Vector3.zero);
        }

        private static AudioClip CreateToneClip(string name, float frequency, float durationSeconds, float gain)
        {
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(SAMPLE_RATE * durationSeconds));
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / SAMPLE_RATE;
                var envelope = 1f - (float)i / sampleCount;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * gain;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Vector3 ToVector3(Unity.Mathematics.float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
    }
}
