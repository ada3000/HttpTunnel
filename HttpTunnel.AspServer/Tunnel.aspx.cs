using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HttpTunnel.AspServer
{
    public partial class Tunnel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string direction = Request.Params["dir"];

        }
    }
}