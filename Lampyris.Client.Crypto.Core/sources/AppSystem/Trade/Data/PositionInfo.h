#pragma once

// STD Include(s)
#include <string>
#include <vector>
#include <stdexcept>
#include <iostream>

// Project Include(s)
#include "Protocol/Protocols.h"
#include "OrderInfo.h"
#include "Util/DateTimeUtil.h"

// PositionInfo ��
class PositionInfo {
public:
    // ���׶ԣ����� BTCUSDT
    std::string symbol;

    // �ֲַ���long �� short
    PositionSide positionSide;

    // �ֲ�����
    double quantity;

    // �ֲ�δʵ��ӯ��
    double unrealizedPnL;

    // �ֲ���ʵ��ӯ��
    double realizedPnL;

    // �ֲֵĳ�ʼ��֤��
    double initialMargin;

    // �ֲֵ�ά�ֱ�֤��
    double maintenanceMargin;

    // �ֲֵĿ��ּ۸�
    double costPrice;

    // ��ǰ��Ǽ۸�
    double markPrice;

    // �ֱֲ��Զ����ֶ���
    int autoDeleveragingLevel;

    // �ֲֵĸ���ʱ��
    std::time_t updateTime;

    // ǿƽ�۸�
    double liquidationPrice;

    // ת��Ϊ PositionBean
    PositionBean toBean() const {
        PositionBean bean;

        bean.set_symbol(this->symbol);
        bean.set_positionside(this->positionSide);
        bean.set_quantity(this->quantity);
        bean.set_unrealizedpnl(this->unrealizedPnL);
        bean.set_realizedpnl(this->realizedPnL);
        bean.set_initialmargin(this->initialMargin);
        bean.set_maintenancemargin(this->maintenanceMargin);
        bean.set_costprice(this->costPrice);
        bean.set_markprice(this->markPrice);
        bean.set_autodeleveraginglevel(this->autoDeleveragingLevel);
        bean.set_updatetime(this->updateTime);
        bean.set_liquidationprice(this->liquidationPrice);
        return bean;
    }

};