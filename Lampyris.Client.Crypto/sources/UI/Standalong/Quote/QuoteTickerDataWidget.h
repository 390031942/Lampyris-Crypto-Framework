#pragma once

#include <QWidget>
#include <QTableWidget>
#include <QVBoxLayout>
#include <QHeaderView>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QJsonDocument>
#include <QJsonArray>
#include <QJsonObject>
#include <QTimer>
#include <unordered_map>
#include <vector>

class QuoteTickerDataWidget : public QWidget {
    Q_OBJECT

public:
    explicit QuoteTickerDataWidget(QWidget* parent = nullptr);

private slots:
    void fetchTickerData(); // 获取数据
    void onReplyFinished(QNetworkReply* reply); // 处理 API 响应
    void onHeaderClicked(int index); // 表头点击事件
    void resetCellColor(int row, int column); // 重置单元格颜色

private:
    QTableWidget* tableWidget; // 表格控件
    QNetworkAccessManager* networkManager; // 网络管理器
    QTimer* updateTimer; // 定时器
    int sortedColumn; // 当前排序的列
    bool ascendingOrder; // 当前排序顺序

    std::unordered_map<QString, int> symbolToRowMap; // 维护 symbol 到表格行号的映射
    std::unordered_map<QString, double> lastPrices; // 存储上一次的价格
    std::vector<std::vector<QTableWidgetItem*>> itemPool; // QTableWidgetItem 池
};
