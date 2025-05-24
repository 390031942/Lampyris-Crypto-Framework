#pragma once

// Ticker数据排序类型
enum class TickerDataSortType {
	NONE, // 默认排序,根据上架时间从早到晚排序
	NAME, // 名称字典序
	PRICE, // 当前价
	CURRENCY,// 成交额
	PERCENTAGE, // 涨跌幅
	RISE_SPEED, // 涨速
};
