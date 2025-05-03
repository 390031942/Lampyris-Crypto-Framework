#pragma once

// Project Include(s)
#include <Base/Singleton.h>
#include "Protocol/Protocols.h"

// STD Include(s)
#include <set>
#include <unordered_map>
#include <algorithm>

class IWebSocketMessageHandler {
public:
	virtual void handleMessage(Response response) = 0;
};

class WebSocketMessageHandlerRegistry:public Singleton<WebSocketMessageHandlerRegistry> {
public:
	void registry(IWebSocketMessageHandler* handler)  {
		if (std::find(m_handlerList.begin(), m_handlerList.end(), handler) != m_handlerList.end()) {
			m_handlerList.push_back(handler);
		}
	}
private:
	void invokeHandler(Response response) {
		for (auto handler : m_handlerList) {
			handler->handleMessage(response);
		}
	}
	std::vector<IWebSocketMessageHandler*> m_handlerList;

	friend class WebSocketClient;
};