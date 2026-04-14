using MediatR;
using TS.Result;

namespace eAppointmentServer.application.Features.Auth.Login;

public sealed record LoginCommand(
    string Username,
    string Password): IRequest<Result<LogincommandResponse>>;
    
    public sealed record LogincommandResponse(
        string Token);
        
        internal sealed class logincommandhandler: IRequestHandler<LoginCommand,Result<LogincommandResponse>>
        
        public async Task<Result<LogincommandResponse>> handle(LoginCommand request) CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }