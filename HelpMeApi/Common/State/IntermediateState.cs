using System.Net;
using HelpMeApi.Common.State.Model;

namespace HelpMeApi.Common.State;

public class IntermediateState <TS>
{
    public StateModel<TS>? Model { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public IntermediateState<TS> Copy(
        StateModel<TS>? model = null,
        HttpStatusCode? statusCode = null)
    {
        Model = model;
        if (statusCode != null)
        {
            StatusCode = (HttpStatusCode)statusCode;
        }

        return this;
    }

    public IntermediateState<TS> Ok(StateModel<TS> model)
    {
        Model = model;
        StatusCode = HttpStatusCode.OK;
        return this;
    }
}
