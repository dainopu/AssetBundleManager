using System.Collections;
using System.Collections.Generic;

namespace HTS_Engine_API
{
	// HTS_Condition: synthesis condition
	public class HTS_Condition
	{
		// global
		public int				sampling_frequency ;		// sampling frequency
		public int				fperiod ;					// frame period
		public double			volume ;					// volume
		public double[]			msd_threshold ;				// MSD thresholds
		public double[]			gv_weight ;					// GV weights

		// duration
		public bool				phoneme_alignment_flag ;	// flag for using phoneme alignment in label
		public double			speed ;						// speech speed

		// spectrum
		public int				stage ;						// if stage=0 then gamma=0 else gamma=-1/stage
		public bool				use_log_gain ;				// log gain flag (for LSP)
		public double			alpha ;						// all-pass constant
		public double			beta ;						// postfiltering coefficient

		// log F0
		public double			additional_half_tone ;		// additional half tone

		// interpolation weights
		public double[]			duration_iw ;				// weights for duration interpolation
		public double[][]		parameter_iw ;				// weights for parameter interpolation
		public double[][]		gv_iw ;						// weights for GV interpolation

		//-----------------------------------------------------------

		public HTS_Condition()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			// global
			sampling_frequency		= 0 ;
			fperiod					= 0 ;
			volume					= 1.0 ;
			msd_threshold			= null ;
			gv_weight				= null ;

			// duration
			speed					= 1.0 ;
			phoneme_alignment_flag	= false ;
			
			// spectrum
			stage					= 0 ;
			use_log_gain			= false ;
			alpha					= 0.0 ;
			beta					= 0.0 ;
			
			// log F0
			additional_half_tone	= 0.0 ;
			
			// interpolation weights
			duration_iw				= null ;
			parameter_iw			= null ;
			gv_iw					= null ;
		}
	}

}