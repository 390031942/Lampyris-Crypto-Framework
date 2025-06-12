#include "QuoteListItem.h"

// QT Include(s)
#include <QApplication>

QuoteListItem::QuoteListItem(QuoteListItemDisplayMode mode, QWidget* parent) : QWidget(parent) {
    // 初始化控件
    m_symbol = new QLabel(this);
    m_24hVolumeCurrency = new AmountText(this);
    m_price = new PriceText(this);
    m_percentage = new PercentageDisplayText(this);
    m_riseSpeed = new PercentageDisplayText(this);

    // 根据模式设置UI布局
    setupUI(mode);

    // 字体设置
    QFont font = QApplication::font();

    // 交易对字体
    font.setPixelSize(18);
    m_symbol->setFont(font);
    m_price->setFont(font);
    m_percentage->setFont(font);

    m_24hVolumeCurrency->setStyleSheet("color: #8A8E97;");
    m_percentage->setDisplayMode(PercentageDisplayText::BackgroundColorMode);
    m_riseSpeed->setDisplayMode(PercentageDisplayText::FontColorMode);
    m_percentage->setFixedWidth(100);

    // 下方的24小时成交额字体
    font.setPixelSize(12);
    m_24hVolumeCurrency->setFont(font);
}

void QuoteListItem::refresh(const QuoteTickerDataPtr tickerData, double minTick) {
    if (tickerData != nullptr) {
        // 测试数据
        m_symbol->setText(tickerData->symbol);
        m_24hVolumeCurrency->setValue(tickerData->currency);
        m_price->setValue(tickerData->price);
        m_price->setMinTick(minTick);
        m_percentage->setValue(tickerData->changePerc);
        m_riseSpeed->setValue(tickerData->riseSpeed);
    }
}

void QuoteListItem::resizeFields(const std::vector<TableColumnWidthInfo>& widthList) {
    int currentX = 0; // 当前列的起始位置

    for (int i = 0; i < m_resizeWidgets.size(); i++) {
        if (i < widthList.size()) {
            auto& widthInfo = widthList[i];
            auto& widget = m_resizeWidgets[i];

            widget->setGeometry(currentX, 0, widthInfo.width, this->height());
            currentX += widthInfo.width;
        }
    }
}

void QuoteListItem::setupUI(QuoteListItemDisplayMode mode) {
    // 无需设置布局，子窗口布局由表头决定

    if (mode == QuoteListItemDisplayMode::SEARCH_LIST) {
        // 搜索列表模式布局: | 符号对 | 价格 | 涨跌幅 |
        m_resizeWidgets.push_back(m_symbol);
        m_resizeWidgets.push_back(m_price);
        m_resizeWidgets.push_back(m_percentage);
    }
    else if (mode == QuoteListItemDisplayMode::MOBILE_QUOTE_LIST) {
        // 移动端行情列表模式布局: 
        // 符号对     |  价格  |    涨跌幅
        // 24h成交额  |  涨速  |

        QWidget* leftWidget = new QWidget(this);
        {
            QVBoxLayout* leftWidgetLayout = new QVBoxLayout(leftWidget);
            leftWidgetLayout->setContentsMargins(0, 0, 0, 0);
            leftWidgetLayout->setSpacing(0);
            leftWidgetLayout->addWidget(m_symbol);
            leftWidgetLayout->addWidget(m_24hVolumeCurrency);
        }

        QWidget* middleWidget = new QWidget(this);
        {
            QVBoxLayout* middleWidgetLayout = new QVBoxLayout(this);
            middleWidgetLayout->setContentsMargins(0, 0, 0, 0);
            middleWidgetLayout->setSpacing(0);
            middleWidgetLayout->addWidget(m_price);
            middleWidgetLayout->addWidget(m_riseSpeed);
        }

        m_resizeWidgets.push_back(leftWidget);
        m_resizeWidgets.push_back(middleWidget);
        m_resizeWidgets.push_back(m_percentage);
    }
    else if (mode == QuoteListItemDisplayMode::STANDALONG_QUOTE_LIST) {
        // PC端行情列表模式布局: | 符号对 | 价格 | 成交额 | 涨跌幅 | 涨速 |
        m_resizeWidgets.push_back(m_symbol);
        m_resizeWidgets.push_back(m_price);
        m_resizeWidgets.push_back(m_24hVolumeCurrency);
        m_resizeWidgets.push_back(m_percentage);
        m_resizeWidgets.push_back(m_riseSpeed);
    }
}
