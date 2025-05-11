#pragma once
// QT Include(s)
#include <QObject>
#include <QWidget>

// STD Include(s)
#include <vector>

// Project Include(s)
#include "UI/Common/StatusBarQuoteItem.h"
#include "UI/Common/TimeWidget.h"
#include "UI/Common/SignalStrengthWidget.h"

class MainStatusBar : public QWidget {
    Q_OBJECT
    const int                        MAX_QUOTE_ITEM_COUNT = 3;
public:
    explicit                         MainStatusBar(QWidget *parent = nullptr);
    void                             reloadQuoteItems();
private:
    std::vector<StatusBarQuoteItem*> m_quoteItemList;
    QHBoxLayout*                     m_layout;
    TimeWidget*                      m_timeWidget;
    SignalStrengthWidget*            m_signalWidget;
    QLabel*                          m_versionLabel;
};