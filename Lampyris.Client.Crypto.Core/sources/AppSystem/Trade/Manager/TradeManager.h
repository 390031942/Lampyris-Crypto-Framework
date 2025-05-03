#pragma once
// Project Include(s)
#include "Network/WebSocketClient.h"
#include "Protocol/Protocols.h"
#include "Base/Singleton.h"
#include "Network/WebSocketMessageHandlerRegistry.h"
#include "Util/DateTimeUtil.h"
#include "Collections/BidirectionalDictionary.h"
#include "../Data/SymbolTradeRule.h"
#include "../Data/OrderInfo.h"
#include "../Data/OrderStatusInfo.h"
#include "../Data/PositionInfo.h"

// STD Include(s)
#include <vector>
#include <queue>
#include <unordered_map>

class TradeTypeConverter {
};

class TradeManager:public SingletonQObject<TradeManager>, public IWebSocketMessageHandler {
public:
	TradeManager();

	virtual void handleMessage(Response response) {
		auto typeCase = response.response_type_case();
		if (typeCase == Response::ResponseTypeCase::kResSubscribeTickerData) {
		}
	}

	#pragma region ��������
public:	
	void placeOrder(OrderInfo orderInfo) {
		OrderBean bean = orderInfo.toBean();
		ReqPlaceOrder reqPlaceOrder;
		reqPlaceOrder.set_allocated_orderbean(&bean);
		WebSocketClient::getInstance()->sendMessage(reqPlaceOrder);
	}
	#pragma endregion

	#pragma region �����޸�
	void modifyOrder(int64_t orderId, OrderInfo orderInfo) {
		OrderBean bean = orderInfo.toBean();
		ReqModifyOrder reqModifyOrder;
		reqModifyOrder.set_orderid(orderId);
		reqModifyOrder.set_allocated_orderbean(&bean);
		WebSocketClient::getInstance()->sendMessage(reqModifyOrder);
	}
	#pragma endregion

	#pragma region ����ȡ��
	void cancelOrder(int64_t orderId) {
		ReqCancelOrder reqCancelOrder;
		reqCancelOrder.set_orderid(orderId);
		WebSocketClient::getInstance()->sendMessage(reqCancelOrder);
	}
	#pragma endregion

	#pragma region һ�����
	void oneKeyClosePosition(const QString& symbol) {
		if (!m_positionInfoMap.contains(symbol))
			return;

		ReqOneKeyClosePosition reqOneKeyClosePosition;
		reqOneKeyClosePosition.add_symbols(symbol.toUtf8());
		WebSocketClient::getInstance()->sendMessage(reqOneKeyClosePosition);
	}

	void oneKeyCloseAllPosition() {
		if (m_positionInfoMap.empty())
			return;

		ReqOneKeyClosePosition reqOneKeyClosePosition;

		for (const auto& pair : m_positionInfoMap) {
			reqOneKeyClosePosition.add_symbols(pair.first.toUtf8());
		}
		WebSocketClient::getInstance()->sendMessage(reqOneKeyClosePosition);
	}
	#pragma endregion

	#pragma region ��ѯ��ǰ����
public:
	/*
	std::vector<const OrderStatusInfo*> queryActiveOrders(const QString& symbol = "") const {
		std::vector<const OrderStatusInfo*> list;
		for (const auto& order : m_activeOrderInfoList) {
			if (symbol.isEmpty() || order.orderInfo.symbol == symbol) {
				list.push_back(&order);
			}
		}

		// ����id�Ӵ�С����
		std::sort(list.begin(), list.end(), [](const OrderStatusInfo* lhs, const OrderStatusInfo* rhs) {
			return lhs->orderId > rhs->orderId;
		});

		return list;
	}
	*/

	void queryActiveOrders(const QString& symbol = "") const {
		ReqQueryActiveOrders reqQueryActiveOrders;
		reqQueryActiveOrders.set_symbol(symbol.toUtf8());
		WebSocketClient::getInstance()->sendMessage(reqQueryActiveOrders);
	}

	#pragma endregion

	#pragma region ��ѯ��ʷ����
	void queryHistoricalOrder(const QString& symbol = "", const QDateTime& beginTime = QDateTime(), const QDateTime& endTime = QDateTime()) {
		ReqQueryHistoricalOrders reqQueryHistoricalOrders;
		reqQueryHistoricalOrders.set_symbol(symbol.toUtf8());

		if (beginTime.isValid()) {
			reqQueryHistoricalOrders.set_begintime(DateTimeUtil::toUtcTimestamp(beginTime));
		}

		if (endTime.isValid()) {
			reqQueryHistoricalOrders.set_endtime(DateTimeUtil::toUtcTimestamp(endTime));
		}

		WebSocketClient::getInstance()->sendMessage(reqQueryHistoricalOrders);
	}
	#pragma endregion

#pragma region ��ѯ�ֲ�
	void queryPosition()
#pragma endregion
	#pragma region ���׹���
private:
	std::unordered_map<QString, SymbolTradeRulePtr> m_symbol2TradeRuleMap;
public:
	const SymbolTradeRulePtr getTradeRule(const QString& symbol) {
		if (m_symbol2TradeRuleMap.contains(symbol)) {
			return m_symbol2TradeRuleMap[symbol];
		}
		return nullptr;
	}
	#pragma endregion

	// ����/������
	void subscribeAll();
	void cancelAllSubscription();
private:
	// Active��������
	std::vector<OrderStatusInfo> m_activeOrderInfoList;

	// Historical��������
	std::vector<OrderStatusInfo> m_historicalOrderInfoList;

	// �ֲ�����
	std::unordered_map<QString, PositionInfo> m_positionInfoMap;
};