using UnityEngine;
namespace GlitchCompiler.Audio { public sealed class AudioService:MonoBehaviour { [SerializeField] AudioSource source; public void Play(AudioClip clip){if(clip!=null)source.PlayOneShot(clip);} } }
