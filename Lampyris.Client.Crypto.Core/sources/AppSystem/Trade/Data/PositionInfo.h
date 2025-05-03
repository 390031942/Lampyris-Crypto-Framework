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

// PositionInfo 类
class PositionInfo {
public:
    // 交易对，例如 BTCUSDT
    std::string symbol;

    // 持仓方向，long 或 short
    PositionSide positionSide;

    // 持仓数量
    double quantity;

    // 持仓未实现盈亏
    double unrealizedPnL;

    // 持仓已实现盈亏
    double realizedPnL;

    // 持仓的初始保证金
    double initialMargin;

    // 持仓的维持保证金
    double maintenanceMargin;

    // 持仓的开仓价格
    double costPrice;

    // 当前标记价格
    double markPrice;

    // 持仓被自动减仓队列
    int autoDeleveragingLevel;

    // 持仓的更新时间
    std::time_t updateTime;

    // 强平价格
    double liquidationPrice;

    // 转换为 PositionBean
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