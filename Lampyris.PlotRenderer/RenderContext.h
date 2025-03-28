#pragma once
#include<span>

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
	std::span<QuoteCandleDataPtr> dataList;

	// ��ǰ��ʾ���׸����ݵ�����
	int                           startIndex;

	// ��ǰѡ�е����ݵ�����
	int                           focusIndex;

	// ����k�ߵĿ��
	int                           width;
};