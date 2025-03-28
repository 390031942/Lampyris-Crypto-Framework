#pragma once
#include<vector>

struct CandleRenderContext {
	// Ԥ�ȼ���õ�����߼ۺ���ͼۣ��Լ����Ӧ������
	double                        maxPrice;
	double                        minPrice;
	int                           maxIndex = -1;
	int                           minIndex = -1;
								  
	// ����̶ȵ����ֵ����Сֵ	  
	double                        gridMaxPrice;
	double                        gridMinPrice;
								  
	double                        gridTextWidth;

	// ��չʾ���ݵ�һ����ͼ
	std::vector<QuoteCandleDataPtr>dataList;

	// ��ǰ��ʾ���׸�/���һ�����ݵ�����
	int                           startIndex;
	int                           endIndex;

	// ��ǰѡ�е����ݵ�����
	int                           focusIndex;

	// ����k�ߵĿ��
	int                           width;

	// �Ƿ�ȴ��ڸ������ʷ���ݼ���
	bool                          isWaitingHistoryData;

	// Ԥ���յ������ݳ���
	int                           expectedSize;

	// �����Ƿ�����
	bool                          isFullData;
};