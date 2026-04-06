// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.VectorData.Tests;

public class VectorBatchProcessorTests
{
    [Fact]
    public async Task BatchAsync_DefaultBatchSize_ReturnsSingleBatchForSmallInput()
    {
        var records = Enumerable.Range(0, 10)
            .Select(i => new VectorRecord { Id = $"chunk-{i}", Content = $"Content {i}", ChunkIndex = i })
            .ToList();

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records))
        {
            batches.Add(batch);
        }

        batches.Should().HaveCount(1);
        batches[0].Should().HaveCount(10);
    }

    [Fact]
    public async Task BatchAsync_ExactMultiple_SplitsEvenly()
    {
        var records = Enumerable.Range(0, 10)
            .Select(i => new VectorRecord { Id = $"chunk-{i}", Content = $"Content {i}", ChunkIndex = i })
            .ToList();

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records, batchSize: 5))
        {
            batches.Add(batch);
        }

        batches.Should().HaveCount(2);
        batches[0].Should().HaveCount(5);
        batches[1].Should().HaveCount(5);
    }

    [Fact]
    public async Task BatchAsync_NotExactMultiple_LastBatchIsSmaller()
    {
        var records = Enumerable.Range(0, 7)
            .Select(i => new VectorRecord { Id = $"chunk-{i}", Content = $"Content {i}", ChunkIndex = i })
            .ToList();

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records, batchSize: 3))
        {
            batches.Add(batch);
        }

        batches.Should().HaveCount(3);
        batches[0].Should().HaveCount(3);
        batches[1].Should().HaveCount(3);
        batches[2].Should().HaveCount(1);
    }

    [Fact]
    public async Task BatchAsync_EmptyInput_ReturnsNoBatches()
    {
        var records = Enumerable.Empty<VectorRecord>();

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records))
        {
            batches.Add(batch);
        }

        batches.Should().BeEmpty();
    }

    [Fact]
    public async Task BatchAsync_SingleRecord_ReturnsSingleBatch()
    {
        var records = new[] { new VectorRecord { Id = "only", Content = "Single", ChunkIndex = 0 } };

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records, batchSize: 5))
        {
            batches.Add(batch);
        }

        batches.Should().HaveCount(1);
        batches[0].Should().HaveCount(1);
        batches[0][0].Id.Should().Be("only");
    }

    [Fact]
    public async Task BatchAsync_BatchSizeOfOne_ReturnsIndividualBatches()
    {
        var records = Enumerable.Range(0, 3)
            .Select(i => new VectorRecord { Id = $"chunk-{i}", Content = $"Content {i}", ChunkIndex = i })
            .ToList();

        var batches = new List<IReadOnlyList<VectorRecord>>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records, batchSize: 1))
        {
            batches.Add(batch);
        }

        batches.Should().HaveCount(3);
        batches.Should().AllSatisfy(b => b.Should().HaveCount(1));
    }

    [Fact]
    public void BatchAsync_NullRecords_ThrowsArgumentNullException()
    {
        var act = async () =>
        {
            await foreach (var _ in VectorBatchProcessor.BatchAsync(null!))
            {
            }
        };

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void BatchAsync_ZeroBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var act = async () =>
        {
            await foreach (var _ in VectorBatchProcessor.BatchAsync([], batchSize: 0))
            {
            }
        };

        act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task BatchAsync_PreservesRecordOrder()
    {
        var records = Enumerable.Range(0, 10)
            .Select(i => new VectorRecord { Id = $"chunk-{i}", Content = $"Content {i}", ChunkIndex = i })
            .ToList();

        var allRecords = new List<VectorRecord>();
        await foreach (var batch in VectorBatchProcessor.BatchAsync(records, batchSize: 3))
        {
            allRecords.AddRange(batch);
        }

        allRecords.Should().HaveCount(10);
        for (int i = 0; i < 10; i++)
        {
            allRecords[i].Id.Should().Be($"chunk-{i}");
            allRecords[i].ChunkIndex.Should().Be(i);
        }
    }
}
