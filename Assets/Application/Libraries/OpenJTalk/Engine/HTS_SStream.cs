using System.Collections;
using System.Collections.Generic;

namespace HTS_Engine_API
{
	public class HTS_SStream
	{
		public int				vector_length ;		// vector length (static features only)
		public double[][]		mean ;				// mean vector sequence
		public double[][]		vari ;				// variance vector sequence
		public double[]			msd ;				// MSD parameter sequence
		public int				win_size ;			// # of windows (static + deltas)
		public int[]			win_l_width ;		// left width of windows
		public int[]			win_r_width ;		// right width of windows
		public double[][]		win_coefficient ;	// window cofficients
		public int[]			win_coefficient_offset ;	// window cofficients offset
		public int				win_max_width ;		// maximum width of windows
		public double[]			gv_mean ;			// mean vector of GV
		public double[]			gv_vari ;			// variance vector of GV
		public bool[]			gv_switch ;			// GV flag sequence
	}

}
