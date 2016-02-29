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
            string id = Request.Params["id"];
            string direction = Request.Params["dir"];
            if (direction == "in")
                State.SetClientInput(id, Request.GetBufferlessInputStream());

            if (direction == "out")
                State.SetClientOutput(id, Response.OutputStream);
        }
    }
}