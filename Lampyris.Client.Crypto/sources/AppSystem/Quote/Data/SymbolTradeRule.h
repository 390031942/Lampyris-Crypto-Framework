#pragma once

// STD Include(s)
#include <string>
#include <cstdint> // 用于 int64_t
#include <cmath>   // 用于 std::isnan 检查
#include <memory>

// QT Include(s)
#include <QString>
#include <QDateTime>

class SymbolTradeRule {
public:
    // 交易对，例如 BTCUSDT
    QString symbol;

    // 最小价格
    double minPrice;

    // 最大价格
    double maxPrice;

    // 价格步进
    double priceStep;

    // 最小数量
    double minQuantity;

    // 最大数量
    double maxQuantity;

    // 数量步进
    double quantityStep;

    // 最小名义价值
    double minNotional;

    // 上架时间
    QDateTime onBoardTime;
};

typedef std::shared_ptr<SymbolTradeRule> SymbolTradeRulePtr;