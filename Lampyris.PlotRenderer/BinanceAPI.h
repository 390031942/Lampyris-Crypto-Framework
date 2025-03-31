#include <QApplication>
#include <QNetworkAccessManager>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QUrl>
#include <QUrlQuery>
#include <QDateTime>
#include <QThreadPool>
#include <QRunnable>
#include <QJsonDocument>
#include <QJsonArray>
#include <QDebug>
#include <QMutex>
#include <QWaitCondition>
#include <vector>
#include <memory>
#include <QSslSocket>

#include "QuoteCandleData.h"

class BinanceAPI : public QObject {
    Q_OBJECT

public:
    explicit BinanceAPI(QObject* parent = nullptr) : QObject(parent) {
        manager = new QNetworkAccessManager(this);
    }

    // fetchKlines 支持多线程分段请求
    void fetchKlines(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime = QDateTime(), const QDateTime& endTime = QDateTime()) {
        executeKlinesRequest(symbol, interval, limit, startTime, endTime);
    }

    // fetchKlinesFromEndTime 支持多线程分段请求
    void fetchKlinesFromEndTime(const QString& symbol, const QString& interval, int limit, const QDateTime& endTime) {
        // 计算每个 interval 的持续时间（以毫秒为单位）
        qint64 intervalDuration = getIntervalDuration(interval);
        if (intervalDuration == 0) {
            qDebug() << "Invalid interval:" << interval;
            return;
        }

        // 计算开始时间
        qint64 startTimeMs = endTime.toMSecsSinceEpoch() - (limit * intervalDuration);
        QDateTime startTime = QDateTime::fromMSecsSinceEpoch(startTimeMs);

        // 调用通用的执行函数
        executeKlinesRequest(symbol, interval, limit, startTime, endTime);
    }

signals:
    // 数据请求完成后发射信号
    void dataFetched(const std::vector<QuoteCandleDataPtr>& dataList);

private:
    QNetworkAccessManager* manager;

    // 通用的 K 线请求执行函数
    void executeKlinesRequest(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime) {
        if (!startTime.isValid() || !endTime.isValid()) {
            // 如果未提供时间范围，直接请求最新的 limit 条数据
            sendKlinesRequest(symbol, interval, limit, QDateTime(), QDateTime());
        }
        else if (limit <= 1000) {
            // 如果 limit 小于等于 1000，直接发送单次请求
            sendKlinesRequest(symbol, interval, limit, startTime, endTime);
        }
        else {
            // 如果 limit 大于 1000，进行分段请求
            int numSegments = (limit + 999) / 1000; // 计算分段数量
            qint64 intervalDuration = getIntervalDuration(interval);
            if (intervalDuration == 0) {
                qDebug() << "Invalid interval:" << interval;
                return;
            }

            QList<QDateTime> segmentStartTimes;
            QList<QDateTime> segmentEndTimes;

            // 计算每段的时间范围
            for (int i = 0; i < numSegments; ++i) {
                QDateTime segmentEndTime = endTime.addMSecs(-i * 1000 * intervalDuration);
                QDateTime segmentStartTime = segmentEndTime.addMSecs(-1000 * intervalDuration);
                segmentStartTimes.append(segmentStartTime);
                segmentEndTimes.append(segmentEndTime);
            }

            // 多线程处理
            QThreadPool threadPool;
            std::vector<QuoteCandleDataPtr> results;
            QMutex mutex;
            QWaitCondition waitCondition;

            for (int i = 0; i < numSegments; ++i) {
                auto runnable = new KlineFetcherRunnable(symbol, interval, 1000, segmentStartTimes[i], segmentEndTimes[i], manager, &results, &mutex, &waitCondition);
                threadPool.start(runnable);
            }

            // 等待所有线程完成
            threadPool.waitForDone();

            // 发射信号，通知数据已完成
            emit dataFetched(results);
        }
    }

    // 单次请求的发送函数
    void sendKlinesRequest(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime) {
        qDebug() << "Supports SSL:" << QSslSocket::supportsSsl();
        qDebug() << "SSL Library Version:" << QSslSocket::sslLibraryVersionString();
        qDebug() << "SSL Build Version:" << QSslSocket::sslLibraryBuildVersionString();
        
        // 构造请求 URL
        QUrl url("https://fapi.binance.com/fapi/v1/klines");
        QUrlQuery query;
        query.addQueryItem("symbol", symbol);
        query.addQueryItem("interval", interval);
        query.addQueryItem("limit", QString::number(limit));
        if (startTime.isValid()) {
            query.addQueryItem("startTime", QString::number(startTime.toMSecsSinceEpoch()));
        }
        if (endTime.isValid()) {
            query.addQueryItem("endTime", QString::number(endTime.toMSecsSinceEpoch()));
        }
        url.setQuery(query);

        QNetworkRequest request(url);

        // 发送 GET 请求
        QNetworkReply* reply = manager->get(request);
        connect(reply, &QNetworkReply::finished, this, [this, reply]() {
            if (reply->error() != QNetworkReply::NoError) {
                qDebug() << "Error:" << reply->errorString();
                reply->deleteLater();
                return;
            }

            // 解析 JSON 数据
            QByteArray responseData = reply->readAll();
            QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
            if (!jsonDoc.isArray()) {
                qDebug() << "Invalid JSON format";
                reply->deleteLater();
                return;
            }

            QJsonArray jsonArray = jsonDoc.array();
            std::vector<QuoteCandleDataPtr> dataList = parseKlinesData(jsonArray);

            // 发射信号，通知数据已完成
            emit dataFetched(dataList);

            reply->deleteLater();
        });
    }

    // 解析 JSON 数据为 QuoteCandleDataPtr 列表
    std::vector<QuoteCandleDataPtr> parseKlinesData(const QJsonArray& jsonArray) {
        std::vector<QuoteCandleDataPtr> dataList;
        for (const QJsonValue& value : jsonArray) {
            QJsonArray item = value.toArray();
            if (item.size() < 6) continue;

            auto data = std::make_shared<QuoteCandleData>();
            data->dateTime = QDateTime::fromMSecsSinceEpoch(item[0].toVariant().toLongLong());
            data->open = item[1].toString().toDouble();
            data->high = item[2].toString().toDouble();
            data->low = item[3].toString().toDouble();
            data->close = item[4].toString().toDouble();
            data->volume = item[5].toString().toDouble();
            if (item.size() > 7) {
                data->currency = item[7].toString().toDouble();
            }

            dataList.push_back(data);
        }
        return dataList;
    }

    // 获取 interval 的持续时间（以毫秒为单位）
    qint64 getIntervalDuration(const QString& interval) {
        static const QMap<QString, qint64> intervalMap = {
            {"1m", 60 * 1000},
            {"3m", 3 * 60 * 1000},
            {"5m", 5 * 60 * 1000},
            {"15m", 15 * 60 * 1000},
            {"30m", 30 * 60 * 1000},
            {"1h", 60 * 60 * 1000},
            {"2h", 2 * 60 * 60 * 1000},
            {"4h", 4 * 60 * 60 * 1000},
            {"6h", 6 * 60 * 60 * 1000},
            {"8h", 8 * 60 * 60 * 1000},
            {"12h", 12 * 60 * 60 * 1000},
            {"1d", 24 * 60 * 60 * 1000},
            {"3d", 3 * 24 * 60 * 60 * 1000},
            {"1w", 7 * 24 * 60 * 60 * 1000},
            {"1M", 30 * 24 * 60 * 60 * 1000}
        };

        return intervalMap.value(interval, 0);
    }

    // 多线程任务类
    class KlineFetcherRunnable : public QRunnable {
    public:
        KlineFetcherRunnable(const QString& symbol, const QString& interval, int limit, const QDateTime& startTime, const QDateTime& endTime, QNetworkAccessManager* manager, std::vector<QuoteCandleDataPtr>* results, QMutex* mutex, QWaitCondition* waitCondition)
            : symbol(symbol), interval(interval), limit(limit), startTime(startTime), endTime(endTime), manager(manager), results(results), mutex(mutex), waitCondition(waitCondition) {}

        void run() override {
            // 构造请求 URL
            QUrl url("https://fapi.binance.com/fapi/v1/klines");
            QUrlQuery query;
            query.addQueryItem("symbol", symbol);
            query.addQueryItem("interval", interval);
            query.addQueryItem("limit", QString::number(limit));
            query.addQueryItem("startTime", QString::number(startTime.toMSecsSinceEpoch()));
            query.addQueryItem("endTime", QString::number(endTime.toMSecsSinceEpoch()));
            url.setQuery(query);

            QNetworkRequest request(url);

            // 发送 GET 请求
            QNetworkReply* reply = manager->get(request);
            QEventLoop loop;
            connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
            loop.exec();

            if (reply->error() != QNetworkReply::NoError) {
                qDebug() << "Error:" << reply->errorString();
                reply->deleteLater();
                return;
            }

            // 解析 JSON 数据
            QByteArray responseData = reply->readAll();
            QJsonDocument jsonDoc = QJsonDocument::fromJson(responseData);
            if (!jsonDoc.isArray()) {
                qDebug() << "Invalid JSON format";
                reply->deleteLater();
                return;
            }

            QJsonArray jsonArray = jsonDoc.array();
            auto dataList = parseKlinesData(jsonArray);

            // 加锁并存储结果
            QMutexLocker locker(mutex);
            results->insert(results->end(), dataList.begin(), dataList.end());
            waitCondition->wakeAll();

            reply->deleteLater();
        }

    private:
        QString symbol;
        QString interval;
        int limit;
        QDateTime startTime;
        QDateTime endTime;
        QNetworkAccessManager* manager;
        std::vector<QuoteCandleDataPtr>* results;
        QMutex* mutex;
        QWaitCondition* waitCondition;

        // 解析 JSON 数据为 QuoteCandleDataPtr 列表
        std::vector<QuoteCandleDataPtr> parseKlinesData(const QJsonArray& jsonArray) {
            std::vector<QuoteCandleDataPtr> dataList;
            for (const QJsonValue& value : jsonArray) {
                QJsonArray item = value.toArray();
                if (item.size() < 6) continue;

                auto data = std::make_shared<QuoteCandleData>();
                data->dateTime = QDateTime::fromMSecsSinceEpoch(item[0].toVariant().toLongLong());
                data->open = item[1].toString().toDouble();
                data->high = item[2].toString().toDouble();
                data->low = item[3].toString().toDouble();
                data->close = item[4].toString().toDouble();
                data->volume = item[5].toString().toDouble();
                if (item.size() > 7) {
                    data->currency = item[7].toString().toDouble();
                }

                dataList.push_back(data);
            }
            return dataList;
        }
    };
};
