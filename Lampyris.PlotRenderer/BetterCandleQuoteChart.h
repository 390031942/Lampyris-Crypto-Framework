// QT Include(s)
#include <QMainWindow>
#include "GridChartWidget.h"
#include "QuoteCandleDataView.h"

namespace Ui {
class BetterChart;
}

class BetterCandleQuoteChart : public QMainWindow {
    Q_OBJECT
public:
    explicit BetterCandleQuoteChart(QWidget *parent = 0);
    ~BetterCandleQuoteChart();

    void setDataView(QuoteCandleDataView* dataView) {
        if (dataView != nullptr) {
            m_dataView = dataView;
        }
    }
private:
    Ui::BetterChart* ui;
    QuoteCandleDataView* m_dataView;
    std::vector<GridChartWidget*> m_chartList;
};
