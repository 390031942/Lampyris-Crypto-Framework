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
    // ����ID
    long long orderId;

    // �����߿ͻ���ID
    int clientUserId = 1;

    // ���׶ԣ����� BTCUSDT
    QString symbol = "";

    // ��������
    OrderSide side;

    // �ֲַ���
    PositionSide positionSide;

    // ��������
    OrderType orderType;

    // �����������Ա��Ϊ��λ��
    double quantity;

    // ������������USDTΪ��λ��
    double cashQuantity;

    // �����۸��޼۵���Ҫ��
    double price;

    // ������Ч��ʽ
    TimeInForceType tifType;

    // TIFΪGTDʱ�������Զ�ȡ��ʱ��
    long long goodTillDate;

    // �Ƿ�ֻ����
    bool reduceOnly;

    // �����б�
    // std::vector<ConditionTriggerData> condition;

    // ����ʱ��
    long long createdTime;

    // �� OrderBean ת��Ϊ OrderInfo
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

    // ת��Ϊ OrderBean
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