using UnityEngine;

namespace RosiesCornerShop.Core
{
    public sealed class ShopAudio : MonoBehaviour
    {
        AudioSource source;
        AudioClip tap, packed;

        void Awake()
        {
            source = gameObject.AddComponent< AudioSource >();
            tap = Create( 680, .07f );
            packed = Create( 960, .18f );
        }

        static AudioClip Create( float pitch, float duration )
        {
            var clip = AudioClip.Create( "Shop chime", (int)( 44100 * duration ), 1, 44100, false );
            var samples = new float[clip.samples];
            for ( int i = 0; i < samples.Length; i++ ) samples[ i ] = Mathf.Sin( i * 2 * Mathf.PI * pitch / 44100 ) * .1f * ( 1f - (float)i / samples.Length );
            clip.SetData( samples, 0 );
            return clip;
        }

        public void Play( bool complete, bool enabledSound )
        {
            if ( enabledSound ) source.PlayOneShot( complete ? packed : tap );
        }

        void OnDestroy()
        {
            if ( tap != null ) Destroy( tap );
            if ( packed != null ) Destroy( packed );
        }
    }
}
