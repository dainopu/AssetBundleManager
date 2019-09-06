using System.Collections;
using System.Collections.Generic;

namespace HTS_Engine_API
{
	// HTS_SMatrices: matrices/vectors used in the speech parameter generation algorithm.
	public class HTS_SMatrices
	{
		public double[][]		mean ;				// mean vector sequence
		public double[][]		ivar ;				// inverse diag variance sequence
		public double[]			g ;					// vector used in the forward substitution
		public double[][]		wuw ;				// W' U^-1 W
		public double[]			wum ;				// W' U^-1 mu
	}
}
