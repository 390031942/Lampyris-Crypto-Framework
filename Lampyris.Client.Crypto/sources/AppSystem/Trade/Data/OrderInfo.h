#pragma once

// STD Include(s)
#include <string>
#include <vector>
#include <stdexcept>
#include <iostream>

// Project Include(s)
#include "Protocol/Protocols.h"

// QT Include(s)
#include <QString>

class OrderInfo {
public:
    // 订单ID
    long long orderId;

    // 创建者客户端ID
    int clientUserId = 1;

    // 交易对，例如 BTCUSDT
    QString symbol = "";

    // 订单方向
    OrderSide side;

    // 持仓方向
    PositionSide positionSide;

    // 订单类型
    OrderType orderType;

    // 订单数量（以标的为单位）
    double quantity;

    // 订单数量（以USDT为单位）
    double cashQuantity;

    // 订单价格（限价单需要）
    double price;

    // 订单有效方式
    TimeInForceType tifType;

    // TIF为GTD时订单的自动取消时间
    long long goodTillDate;

    // 是否只减仓
    bool reduceOnly;

    // 条件列表
    // std::vector<ConditionTriggerData> condition;

    // 创建时间
    long long createdTime;

    // 从 OrderBean 转换为 OrderInfo
    static OrderInfo valueOf(const OrderBean& bean) {
        OrderInfo orderInfo;
        orderInfo.orderId = -1;
        orderInfo.clientUserId = -1;
        orderInfo.symbol = QString::fromStdString(bean.symbol());
        orderInfo.side = bean.side();
        orderInfo.positionSide = bean.positionside();
        orderInfo.orderType = bean.ordertype();
        orderInfo.quantity = bean.quantity();
        orderInfo.cashQuantity = bean.cashquantity();
        orderInfo.price = bean.price();
        orderInfo.tifType = bean.tiftype();
        orderInfo.goodTillDate = bean.goodtilldate();
        orderInfo.reduceOnly = bean.reduceonly();
        orderInfo.createdTime = bean.createdtime();

        // for (const auto& cond : bean.condition()) {
        //     orderInfo.condition.push_back(ConditionTriggerData{
        //         cond.type,
        //         cond.value
        //     });
        // }

        return orderInfo;
    }

    // 转换为 OrderBean
    OrderBean toBean() const {
        OrderBean bean;
        bean.set_symbol(this->symbol.toUtf8());
        bean.set_side(this->side);
        bean.set_positionside(this->positionSide);
        bean.set_ordertype(this->orderType);
        bean.set_quantity(this->quantity);
        bean.set_cashquantity(this->cashQuantity);
        bean.set_price(this->price);
        bean.set_tiftype(this->tifType);
        bean.set_goodtilldate(this->goodTillDate);
        bean.set_reduceonly(this->reduceOnly);
        bean.set_createdtime(this->createdTime);

        // for (const auto& cond : this->condition) {
        //     bean.condition().push(ConditionTriggerData{
        //         cond.type,
        //         cond.value
        //     });
        // }

        return bean;
    }
};