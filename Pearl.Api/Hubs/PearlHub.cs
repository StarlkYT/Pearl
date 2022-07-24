﻿using Microsoft.AspNetCore.SignalR;
using Pearl.Api.Extensions;
using Pearl.Api.Models.Responses;
using Pearl.Api.Services;

namespace Pearl.Api.Hubs;

public sealed class PearlHub : Hub
{
    private readonly PearlService pearlService;

    public PearlHub(PearlService pearlService)
    {
        this.pearlService = pearlService;
    }

    [HubMethodName("JoinGroup")]
    public async Task<ErrorResponse?> JoinGroupAsync(string name)
    {
        var response = await pearlService.JoinGroupAsync(name, Context.Subject());

        return response.IfSuccess(async () =>
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, name);
            await Clients.Group(name).SendAsync("Group", response.Value);
        });
    }

    public ErrorResponse? SendMessage(string content, string groupName)
    {
        var subject = Context.Subject();
        var response = pearlService.SendMessage(content, groupName, subject);

        return response.IfSuccess(async () =>
            await Clients.Group(groupName).SendAsync("Message", subject, response.Value));
    }
}