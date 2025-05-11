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
 * K线数据视图类，一个K线图UI可以设置一个QuoteCandleDataView来显示k线图
 * 一个k线视图由于多个QuoteCandleData数据段组成。
 * 当k线图显示的数量大于所有数据段的总长度时，需要向服务端请求新的数据段，并追加到segments中。
 * 分成多个数据段的原因，是因为服务端返回的数据存在最大的长度。
 * 
 * 此外，该类还需要实现k线UI浏览的相关操作，如键盘左右键向左向右移动k线，上下键缩放k线。
 * 同时，还需支持k线聚焦(鼠标移动到某根k线)，k线区间统计(鼠标选中的k线区域)等功能。
 * 
 * 最后，k线视图类需要支持根据K线数据 计算出相关指标数据，并获取
 * 
 * 分段示意(从左到右表示数据从早到晚),假设n = m_segment.size()
 * |m_segments(n - 1)|m_segments(n - 2)| ... | m_segments(0) | m_dynamicSegment
 */ 
class QuoteCandleDataView {
private:
    QString m_symbol;

    BarSize m_barSize;

    // 分段的完整 K 线数据
    std::vector<QuoteCandleDataSegmentPtr> m_segments;

    // 动态分段(实时行情中的更新)
    QuoteCandleDataDynamicSegmentPtr m_dynamicSegment;

    // 视图中展示的 K 线数量
    int m_displaySize;                                  

    // 当前视图的起始索引（在完整数据中的偏移量）
    int m_startIndex;

    // 当前聚焦的索引（在当前可见数据区间中的索引,相对于m_startIndex）
    int m_focusIndex;

    // 最后一个segment拥有的数据的长度
    int m_lastSegmentSize; 

    // 是否在下载数据
    bool m_isLoading = false;

    // 历史数据是否完备，如果为true，说明expand时无需再请求数据
    bool m_isFullData = false;
public:
    QuoteCandleDataView(size_t m_displaySize);

    ~QuoteCandleDataView();

    // 向左移动视图
    void moveLeft();

    // 向右移动视图
    void moveRight();

    void setFocusIndex(int index);

    void expand(int displaySize);

    void shrink(int displaySize);

    // 获取视图中展示的 K 线数量
    int getDisplaySize() const;

    // 获取完整数据的总大小
    size_t getTotalSize() const;

    // 根据全局索引获取 K 线数据
    const QuoteCandleData& getCandleDataByGlobalIndex(int globalIndex) const;

    // 运算符[]：根据视图中的索引返回 K 线数据
    const QuoteCandleData& operator[](int index) const;

    void notifyDataReceived();

    // 获取最后一个数据对应的DateTime
    QDateTime getFirstDataDateTime();

    friend class QuoteManager;
};
