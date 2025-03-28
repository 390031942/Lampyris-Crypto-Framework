#pragma once
#include "QuoteCandleData.h"
#include <QCoreApplication>
#include <QNetworkAccessManager>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QJsonDocument>
#include <QJsonArray>
#include <QJsonObject>
#include <QUrl>
#include <QUrlQuery>
#include <memory>
#include <vector>
#include <iostream>

class BinanceAPI : public QObject {
    Q_OBJECT

public:
    explicit BinanceAPI(QObject* parent = nullptr) : QObject(parent) {
        manager = new QNetworkAccessManager(this);
    }

    void fetchKlines(const QString& symbol, const QString& interval, int limit) {
        // �������� URL
        QUrl url("https://fapi.binance.com/fapi/v1/klines");
        QUrlQuery query;
        query.addQueryItem("symbol", symbol);
        query.addQueryItem("interval", interval);
        query.addQueryItem("limit", QString::number(limit));
        url.setQuery(query);

        QNetworkRequest request(url);

        // ���� GET ����
        QNetworkReply* reply = manager->get(request);
        connect(reply, &QNetworkReply::finished, this, [this, reply]() {
            handleReply(reply);
            });
    }

signals:
    void dataFetched(const std::vector<QuoteCandleDataPtr>& data);

private slots:
    void handleReply(QNetworkReply* reply) {
        if (reply->error() != QNetworkReply::NoError) {
            std::cerr << "Error: " << reply->errorString().toStdString() << std::endl;
            reply->deleteLater();
            return;
        }

        // ���� JSON ����
        QByteArray responseData = reply->readAll();
        QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
        if (!jsonDoc.isArray()) {
            std::cerr << "Invalid JSON format" << std::endl;
            reply->deleteLater();
            return;
        }

        QJsonArray jsonArray = jsonDoc.array();
        std::vector<QuoteCandleDataPtr> candleDataList;

        for (const QJsonValue& value : jsonArray) {
            if (!value.isArray()) continue;

            QJsonArray candleArray = value.toArray();
            auto candleData = std::make_shared<QuoteCandleData>();

            // Binance K�����ݸ�ʽ��
            // [
            //   0: ����ʱ��,
            //   1: ���̼�,
            //   2: ��߼�,
            //   3: ��ͼ�,
            //   4: ���̼�,
            //   5: �ɽ���,
            //   6: ����ʱ��,
            //   7: �ɽ���,
            //   ...
            // ]
            candleData->dateTime = QDateTime::fromMSecsSinceEpoch(candleArray[0].toVariant().toLongLong()).toString(Qt::ISODate);
            candleData->open = candleArray[1].toString().toDouble();
            candleData->high = candleArray[2].toString().toDouble();
            candleData->low = candleArray[3].toString().toDouble();
            candleData->close = candleArray[4].toString().toDouble();
            candleData->volume = candleArray[5].toString().toDouble();
            candleData->currency = candleArray[7].toString().toDouble();

            candleDataList.push_back(candleData);
        }

        emit dataFetched(candleDataList);

        reply->deleteLater();
    }

private:
    QNetworkAccessManager* manager;
};

