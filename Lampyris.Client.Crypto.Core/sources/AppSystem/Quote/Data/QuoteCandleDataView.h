#pragma once

// STD Include(s)
#include <vector>
#include <stdexcept>
#include <iostream>
#include <algorithm>  // std::lower_bound

// QT Include(s)
#include <QDateTime>

// Project Include(s)
#include "QuoteCandleData.h"
#include "AppSystem/Quote/Const/BarSize.h"

/*
 * K��������ͼ�࣬һ��K��ͼUI��������һ��QuoteCandleDataView����ʾk��ͼ
 * һ��k����ͼ���ڶ��QuoteCandleData���ݶ���ɡ�
 * ��k��ͼ��ʾ�����������������ݶε��ܳ���ʱ����Ҫ�����������µ����ݶΣ���׷�ӵ�segments�С�
 * �ֳɶ�����ݶε�ԭ������Ϊ����˷��ص����ݴ������ĳ��ȡ�
 * 
 * ���⣬���໹��Ҫʵ��k��UI�������ز�������������Ҽ����������ƶ�k�ߣ����¼�����k�ߡ�
 * ͬʱ������֧��k�߾۽�(����ƶ���ĳ��k��)��k������ͳ��(���ѡ�е�k������)�ȹ��ܡ�
 * 
 * ���k����ͼ����Ҫ֧�ָ���K������ ��������ָ�����ݣ�����ȡ
 * 
 * �ֶ�ʾ��(�����ұ�ʾ���ݴ��絽��),����n = m_segment.size()
 * |m_segments(n - 1)|m_segments(n - 2)| ... | m_segments(0) | m_dynamicSegment
 */ 
class QuoteCandleDataView {
private:
    QString m_symbol;

    BarSize m_barSize;

    // �ֶε����� K ������
    std::vector<QuoteCandleDataSegmentPtr> m_segments;

    // ��̬�ֶ�(ʵʱ�����еĸ���)
    QuoteCandleDataDynamicSegmentPtr m_dynamicSegment;

    // ��ͼ��չʾ�� K ������
    int m_displaySize;                                  

    // ��ǰ��ͼ����ʼ�����������������е�ƫ������
    int m_startIndex;

    // ��ǰ�۽����������ڵ�ǰ�ɼ����������е�����,�����m_startIndex��
    int m_focusIndex;

    // ���һ��segmentӵ�е����ݵĳ���
    int m_lastSegmentSize; 

    // �Ƿ�����������
    bool m_isLoading = false;

    // ��ʷ�����Ƿ��걸�����Ϊtrue��˵��expandʱ��������������
    bool m_isFullData = false;
public:
    QuoteCandleDataView(size_t m_displaySize);

    ~QuoteCandleDataView();

    // �����ƶ���ͼ
    void moveLeft();

    // �����ƶ���ͼ
    void moveRight();

    void setFocusIndex(int index);

    void expand(int displaySize);

    void shrink(int displaySize);

    // ��ȡ��ͼ��չʾ�� K ������
    int getDisplaySize() const;

    // ��ȡ�������ݵ��ܴ�С
    size_t getTotalSize() const;

    // ����ȫ��������ȡ K ������
    const QuoteCandleData& getCandleDataByGlobalIndex(int globalIndex) const;

    // �����[]��������ͼ�е��������� K ������
    const QuoteCandleData& operator[](int index) const;

    void notifyDataReceived();

    // ��ȡ���һ�����ݶ�Ӧ��DateTime
    QDateTime getFirstDataDateTime();

    friend class QuoteManager;
};
