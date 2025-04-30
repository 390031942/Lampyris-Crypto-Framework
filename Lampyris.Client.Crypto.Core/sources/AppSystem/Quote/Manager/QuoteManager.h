#pragma once
// Project Include(s)
#include "Network/WebSocketClient.h"
#include "Protocol/Protocols.h"
#include "Base/Singleton.h"
#include "Network/WebSocketClient.h"

class QuoteManager:public SingletonQObject<QuoteManager> {
public:
	void subscribeTickerData() {
		ReqSubscribeTickerData reqSubscribeTickerData;
		reqSubscribeTickerData.set_iscancel(false);
		WebSocketClient::getInstance()->sendMessage(reqSubscribeTickerData);
	}

	void cancelSubscribeTickerData() {
		ReqSubscribeTickerData reqSubscribeTickerData;
		reqSubscribeTickerData.set_iscancel(true);
		WebSocketClient::getInstance()->sendMessage(reqSubscribeTickerData);
	}
};