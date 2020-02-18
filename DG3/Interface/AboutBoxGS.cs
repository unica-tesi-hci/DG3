using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestureStudio
{
	partial class AboutBoxGS : Form
	{
		public static string G3Citation = @"L. A. Leiva, D. Martín-Albo, R. Plamondon (2015). Gestures à Go Go: Authoring Synthetic Human-like Stroke Gestures Using the Kinematic Theory of Rapid Movements. ACM Transactions on Intelligent Systems and Technology, 7(2). https://dl.acm.org/doi/10.1145/2799648";
		public static string DollarCopyright = @"The $1 Unistroke Recognizer (C# version)

		Jacob O. Wobbrock, Ph.D.
		The Information School
		University of Washington
		Mary Gates Hall, Box 352840
		Seattle, WA 98195-2840
		wobbrock@u.washington.edu

		Andrew D. Wilson, Ph.D.
		Microsoft Research
		One Microsoft Way
		Redmond, WA 98052
		awilson@microsoft.com

		Yang Li, Ph.D.
		Department of Computer Science and Engineering
		University of Washington
		The Allen Center, Box 352350
		Seattle, WA 98195-2840
		yangli@cs.washington.edu

The Protractor enhancement was published by Yang Li and programmed here by 
Jacob O. Wobbrock.

	Li, Y. (2010). Protractor: A fast and accurate gesture 
	  recognizer. Proceedings of the ACM Conference on Human 
	  Factors in Computing Systems (CHI '10). Atlanta, Georgia
	  (April 10-15, 2010). New York: ACM Press, pp. 2169-2172.

The $1 Unistroke Recognizer (C# version) is distributed under the ""New BSD License"" agreement:

Copyright (c) 2007-2011, Jacob O. Wobbrock, Andrew D. Wilson and Yang Li.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in the
     documentation and/or other materials provided with the distribution.
   * Neither the names of the University of Washington nor Microsoft,
     nor the names of its contributors may be used to endorse or promote 
     products derived from this software without specific prior written
     permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS
IS"" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock OR Andrew D. Wilson
OR Yang Li BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.";

		public static string QCopyright = @"The $Q Point-Cloud Recognizer (.NET Framework C# version)

 	    Radu-Daniel Vatavu, Ph.D.
	    University Stefan cel Mare of Suceava
	    Suceava 720229, Romania
	    radu.vatavu@usm.ro

	    Lisa Anthony, Ph.D.
		Department of CISE
		University of Florida
		Gainesville, FL 32611, USA
		lanthony@cise.ufl.edu

	    Jacob O. Wobbrock, Ph.D.
 	    The Information School
	    University of Washington
	    Seattle, WA 98195-2840
	    wobbrock@uw.edu

The academic publication for the $Q recognizer, and what should be 
used to cite it, is:

	Vatavu, R.-D., Anthony, L. and Wobbrock, J.O. (2018).  
	  $Q: A Super-Quick, Articulation-Invariant Stroke-Gesture
   Recognizer for Low-Resource Devices. Proceedings of 20th International Conference on
   Human-Computer Interaction with Mobile Devices and Services (MobileHCI '18). Barcelona, Spain
	  (September 3-6, 2018). New York: ACM Press.
	  DOI: https://doi.org/10.1145/3229434.3229465

The $Q Point-Cloud Recognizer (.NET Framework C# version) is distributed under the ""New BSD License"" agreement:

Copyright(c) 2018, Radu-Daniel Vatavu, Lisa Anthony, and
Jacob O.Wobbrock.All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
   * Redistributions of source code must retain the above copyright
	 notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright
	 notice, this list of conditions and the following disclaimer in the
	 documentation and/or other materials provided with the distribution.
   * Neither the names of the University Stefan cel Mare of Suceava,
		University of Washington, nor University of Florida, nor the names of its contributors

		may be used to endorse or promote products derived from this software
		without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS
IS"" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL Radu-Daniel Vatavu OR Lisa Anthony
OR Jacob O. Wobbrock BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
SUCH DAMAGE.";

		public AboutBoxGS()
		{
			InitializeComponent();
			this.Text = String.Format("About {0}", AssemblyTitle);
			this.labelProductName.Text = AssemblyProduct;
			this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
			this.labelCopyright.Text = AssemblyCopyright;
			this.labelCompanyName.Text = AssemblyCompany;
			this.textBoxDescription.Text = $"Gestures A GoGo:{Environment.NewLine}{G3Citation}{Environment.NewLine}{Environment.NewLine}$1 copyright notice:{Environment.NewLine}{DollarCopyright}{Environment.NewLine}{Environment.NewLine}$Q Point-Cloud Recognizer copyright notice:{Environment.NewLine}{QCopyright}";
			
		}

		#region Assembly Attribute Accessors

		public string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion
	}
}
