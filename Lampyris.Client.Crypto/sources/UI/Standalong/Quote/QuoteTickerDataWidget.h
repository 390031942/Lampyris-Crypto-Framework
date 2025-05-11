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
    void fetchTickerData(); // ��ȡ����
    void onReplyFinished(QNetworkReply* reply); // ���� API ��Ӧ
    void onHeaderClicked(int index); // ��ͷ����¼�
    void resetCellColor(int row, int column); // ���õ�Ԫ����ɫ

private:
    QTableWidget* tableWidget; // ���ؼ�
    QNetworkAccessManager* networkManager; // ���������
    QTimer* updateTimer; // ��ʱ��
    int sortedColumn; // ��ǰ�������
    bool ascendingOrder; // ��ǰ����˳��

    std::unordered_map<QString, int> symbolToRowMap; // ά�� symbol ������кŵ�ӳ��
    std::unordered_map<QString, double> lastPrices; // �洢��һ�εļ۸�
    std::vector<std::vector<QTableWidgetItem*>> itemPool; // QTableWidgetItem ��
};
