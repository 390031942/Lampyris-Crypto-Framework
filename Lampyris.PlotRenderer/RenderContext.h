#pragma once
#include<vector>

struct CandleRenderContext {
	// 预先计算得到的最高价和最低价，以及其对应的索引
	double                        maxPrice;
	double                        minPrice;
	int                           maxIndex = -1;
	int                           minIndex = -1;
								  
	// 网格刻度的最大值和最小值	  
	double                        gridMaxPrice;
	double                        gridMinPrice;
								  
	double                        gridTextWidth;

	// 待展示数据的一个视图
	std::vector<QuoteCandleDataPtr>dataList;

	// 当前显示的首个/最后一个数据的索引
	int                           startIndex;
	int                           endIndex;

	// 当前选中的数据的索引
	int                           focusIndex;

	// 单根k线的宽度
	int                           width;

	// 是否等待在更早的历史数据加载
	bool                          isWaitingHistoryData;

	// 预期收到的数据长度
	int                           expectedSize;

	// 数据是否完整
	bool                          isFullData;
};