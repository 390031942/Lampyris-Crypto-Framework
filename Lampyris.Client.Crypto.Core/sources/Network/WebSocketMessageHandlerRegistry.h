#pragma once

// Project Include(s)
#include <Base/Singleton.h>
#include "Protocol/Protocols.h"

// STD Include(s)
#include <set>
#include <unordered_map>
#include <functional>


class IWebSocketMessageHandler {
public:
	virtual void handleMessage(Response::ResponseTypeCase type, Response response) = 0;
};

class WebSocketMessageHandlerRegistry:public Singleton<WebSocketMessageHandlerRegistry> {
	
};