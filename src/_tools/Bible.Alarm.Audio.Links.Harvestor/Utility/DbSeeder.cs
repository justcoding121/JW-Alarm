using Bible.Alarm.Audio.Links.Harvestor.Utility;
using JW.Alarm.Models;
using JW.Alarm.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Audio.Links.Harvestor
{
    public class DbSeeder
    {
        public async static Task Seed(string indexDir)
        {
            await seedBibleTranslations(indexDir);
        }

        private async static Task seedBibleTranslations(string indexDir)
        {
            using (var db = new MediaDbContext())
            {
                db.Database.Migrate();

                var mediaReader = new MediaReader(indexDir);

                var bibleLanguages = await mediaReader.GetBibleLanguages();

                foreach (var language in bibleLanguages)
                {
                    var newLanguage = await db.Languages.FirstOrDefaultAsync(x => x.Code == language.Value.Code
                                                                            && x.Name == language.Value.Name);
                    if (newLanguage == null)
                    {
                        newLanguage = new Language()
                        {
                            Code = language.Value.Code,
                            Name = language.Value.Name
                        };
                    }

                    var translations = await mediaReader.GetBibleTranslations(language.Key);

                    foreach (var translation in translations)
                    {
                        var bibleTranslation = new BibleTranslation()
                        {
                            Name = translation.Value.Name,
                            Code = translation.Value.Code,
                            Language = newLanguage
                        };

                        var books = await mediaReader.GetBibleBooks(language.Key, translation.Key);

                        foreach (var book in books)
                        {
                            var newBook = new BibleBook()
                            {
                                Name = book.Value.Name,
                                Number = book.Value.Number
                            };

                            bibleTranslation.Books.Add(newBook);

                            var chapters = await mediaReader.GetBibleChapters(language.Key, translation.Key, book.Key);

                            foreach (var chapter in chapters)
                            {
                                var newChapter = new BibleChapter()
                                {
                                    Number = chapter.Value.Number,
                                    Duration = chapter.Value.Duration,
                                    Url = chapter.Value.Url
                                };

                                newBook.Chapters.Add(newChapter);
                            }
                        }

                        db.BibleTranslations.Add(bibleTranslation);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        private async static Task seedSongBooks(string indexDir)
        {
            using (var db = new MediaDbContext())
            {
                db.Database.Migrate();

                var mediaReader = new MediaReader(indexDir);

                var melodyMusicReleases = await mediaReader.GetMelodyMusicReleases();

                foreach (var melodyMusicRelease in melodyMusicReleases)
                {
                    var translations = await mediaReader.GetMelodyMusicTracks(melodyMusicRelease.Key);

                    foreach (var translation in translations)
                    {
                        var bibleTranslation = new BibleTranslation()
                        {
                            Name = translation.Value.Name,
                            Code = translation.Value.Code,
                            Language = newLanguage
                        };

                        var books = await mediaReader.GetBibleBooks(melodyMusicRelease.Key, translation.Key);

                        foreach (var book in books)
                        {
                            var newBook = new BibleBook()
                            {
                                Name = book.Value.Name,
                                Number = book.Value.Number
                            };

                            bibleTranslation.Books.Add(newBook);

                            var chapters = await mediaReader.GetBibleChapters(melodyMusicRelease.Key, translation.Key, book.Key);

                            foreach (var chapter in chapters)
                            {
                                var newChapter = new BibleChapter()
                                {
                                    Number = chapter.Value.Number,
                                    Duration = chapter.Value.Duration,
                                    Url = chapter.Value.Url
                                };

                                newBook.Chapters.Add(newChapter);
                            }
                        }

                        db.BibleTranslations.Add(bibleTranslation);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
