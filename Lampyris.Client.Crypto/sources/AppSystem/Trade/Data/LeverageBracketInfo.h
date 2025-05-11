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
class LeverageBracketInfo {
public:
    // 交易对，例如 BTCUSDT
    std::string symbol;

    // 交易对
    int leverage;

    // 持仓数量
    double quantity;

    // 当前分层下的名义价值上限
    double notionalCap;

    // 当前分层下的名义价值下限
    double notionalFloor;
};